module HardRightEdge.Repositories

open System
open System.Net
open System.Data.Common
open Option
open Microsoft.FSharp.Core.Operators.Unchecked

open HardRightEdge.Domain
open HardRightEdge.Data
open HardRightEdge.Infrastructure.Common

module Shares =
    
  let private saveSharePlatform (sharePlatform: SharePlatform) =
    use db = new Db ()
    use cmd = db?Sql <- "INSERT INTO    share_platform
                                        ( share_id,
                                          platform_id,
                                          symbol )
                        VALUES          ( :share_id,
                                          :platform_id,
                                          :symbol )
                        ON CONFLICT     ( symbol,
                                          platform_id) 
                        DO UPDATE          
                        SET share_id = :share_id"
    db.Open()
    cmd?share_id            <- sharePlatform.shareId
    cmd?platform_id         <- int sharePlatform.platform
    cmd?symbol              <- sharePlatform.symbol
    cmd.ExecuteNonQuery ()  |> ignore
    sharePlatform

  let private getSharePriceByShare (shareId: int64) (days: int) (db: Db) =
    use cmd = db?Sql <- (sprintf "SELECT    id,
                                            share_id,
                                            date,
                                            openp,
                                            high,
                                            low,
                                            close,
                                            volume,
                                            adj_close
                                  FROM      share_price
                                  WHERE     share_id = :share_id
                                  ORDER BY  id DESC                                  
                                  LIMIT     %i" days)

    if db.IsClosed then db.Open()
    
    cmd?share_id <- shareId
    
    use rdr = cmd.ExecuteReader()
    if rdr.Read()
    then          
      Some { id       = ofObj rdr?id
             shareId  = ofObj rdr?share_id
             date     = rdr?date
             openp    = rdr?openp
             high     = rdr?high
             low      = rdr?low
             close    = rdr?close
             adjClose = ofObj rdr?adj_close
             volume   = rdr?volume }
    else 
      None

  let private getSharePriceByShare' (shareId: int64) (days: int) =
    use db = new Db()
    getSharePriceByShare shareId days db

  let update (share: Share) =
    use db = new Db ()
    use cmd = db?Sql <- "UPDATE share
                          SET name = :name,
                              previous_name = :previous_name
                          where id = :id"

    db.Open ()
    cmd?name <- share.name
    cmd?id <- share.id
    cmd?previous_name <- share.previousName
    cmd.ExecuteNonQuery () |> ignore
    share
    
  let insert (share: Share) =
    use db = new Db ()
    use cmd = db?Sql <- "INSERT INTO  share
                                      ( name )
                        VALUES        ( :name );

                        SELECT CURRVAL(pg_get_serial_sequence('share','id'));"
    db.Open ()
    cmd?name <- share.name
    { share with id = Some (unbox<int64> (cmd.ExecuteScalar())) }

  let save (share: Share) =
    let shr = match share with
              | { id = Some _ } -> update share
              | _               -> insert share

    let savedShare = {  shr with                        
                          platforms = [| for sharePlatform in share.platforms ->
                                            saveSharePlatform { sharePlatform with shareId = shr.id } |] }
    use db = new Db ()
    use cmd = db?Sql <- "INSERT INTO  share_price 
                                      ( share_id, 
                                        date, 
                                        openp, 
                                        high, 
                                        low, 
                                        close, 
                                        volume, 
                                        adj_close  ) 
                          VALUES      ( :share_id, 
                                        :date, 
                                        :openp, 
                                        :high, 
                                        :low, 
                                        :close, 
                                        :volume, 
                                        :adj_close );

                        SELECT CURRVAL(pg_get_serial_sequence('share_price','id'));"
    
    db.Open ()
    { savedShare with prices = [  for price in share.prices do
                                    cmd?share_id  <- shr.id
                                    cmd?date      <- price.date
                                    cmd?openp     <- price.openp
                                    cmd?high      <- price.high
                                    cmd?low       <- price.low
                                    cmd?close     <- price.close
                                    cmd?volume    <- price.volume
                                    cmd?adj_close <- price.adjClose                                    
                                    yield { price with id = Some (cmd.ExecuteScalar<int64>()); shareId = shr.id } ] }

  let get (id: int64) =
    // TODO: Adapt the following code to work with multiple 
    // Platforms, that will cause a Share to be returned 
    // for each Platform.

    use db = new Db()
    use cmd = db?Sql <- "SELECT     share.id,
                                    name,
                                    previous_name,
                                    platform_id,
                                    symbol
                          FROM      share
                          LEFT JOIN share_platform
                          ON        share_platform.share_id = share.id
                          WHERE     share.id = :id
                          ORDER BY  share.id"
    cmd?id <- id

    db.Open()
    use rdr = cmd.ExecuteReader()

    if rdr.Read()
    then Some { id            = ofObj rdr?id
                name          = rdr?name
                currency      = None
                previousName  = rdr?previous_name
                // TODO: Call ShareRepository.getSharePriceByShare 1L |> Seq.take 1 |> Seq.toList
                // This will only include the latest share price.
                // The AppService can then fetch all new prices from this one until today
                // and save them.
                prices        = [] // [ (Seq.head (getSharePriceByShare id)) ]
                platforms     = Seq.empty<SharePlatform> }
    else None

  // TODO: Tests this
  let getBySymbol (symbol: string) (platform: Platform) =
    use db = new Db()
    use cmd = db?Sql <- "SELECT       share.id,
                                      share.name,
                                      share.previous_name,
                                      share_platform.platform_id,
                                      share_platform.symbol

                          FROM        share

                          INNER JOIN  share_platform
                          ON          share_platform.share_id = share.id

                          WHERE       share_platform.symbol = :symbol
                          AND         share_platform.platform_id = :platform_id

                          LIMIT 1"
    cmd?symbol      <- symbol
    cmd?platform_id <- int platform

    db.Open()
    let share = using (cmd.ExecuteReader()) (fun rdr ->
                if rdr.Read()
                then
                  Some {  id            = Some rdr?id
                          name          = rdr?name
                          currency      = None
                          previousName  = rdr?previous_name
                          prices        = List.empty<SharePrice>
                          platforms     = seq { yield { shareId = Some rdr?id
                                                        platform = enum rdr?platform_id
                                                        symbol = rdr?symbol } } }
                else
                  None)

    match share with
    | Some ({ id = Some id' } as s) -> 
      Some { s with prices =  match getSharePriceByShare id' 1 db with
                              | Some sp -> [ sp ]
                              | _ -> [] }
    | None -> None

// This is for the DB
// Integration.Saxo is for the Excel
module TransactionRepository =

  let x = 5