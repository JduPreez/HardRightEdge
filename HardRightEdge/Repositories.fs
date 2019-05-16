module HardRightEdge.Repositories

open System
open System.Net
open System.Data.Common
open Option
open Microsoft.FSharp.Core.Operators.Unchecked

open HardRightEdge.Domain
open HardRightEdge.Data
open HardRightEdge.Infrastructure.Common

module Securities =
    
  let private saveSharePlatform (sharePlatform: SecurityPlatform) =
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
    cmd?share_id            <- sharePlatform.securityId
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
    [ while rdr.Read() do      
        yield { id       = ofObj rdr?id
                securityId  = ofObj rdr?share_id
                date     = rdr?date
                openp    = rdr?openp
                high     = rdr?high
                low      = rdr?low
                close    = rdr?close
                adjClose = ofObj rdr?adj_close
                volume   = rdr?volume } ]

  let private getSharePriceByShare' (shareId: int64) (days: int) =
    use db = new Db()
    getSharePriceByShare shareId days db

  let update (share: Security) =
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
    
  let insert (share: Security) =
    use db = new Db ()
    use cmd = db?Sql <- "INSERT INTO  share
                                      ( name )
                        VALUES        ( :name );

                        SELECT CURRVAL(pg_get_serial_sequence('share','id'));"
    db.Open ()
    cmd?name <- share.name
    { share with id = Some (unbox<int64> (cmd.ExecuteScalar())) }

  let save (security: Security) =
    let sec = match security with
              | { id = Some _ } -> update security
              | _               -> insert security

    let savedShare = {  sec with                        
                          platforms = [| for securityPlatform in security.platforms ->
                                            saveSharePlatform { securityPlatform with securityId = sec.id } |] }
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
    { savedShare with prices = [  for price in security.prices do
                                    cmd?share_id  <- sec.id
                                    cmd?date      <- price.date
                                    cmd?openp     <- price.openp
                                    cmd?high      <- price.high
                                    cmd?low       <- price.low
                                    cmd?close     <- price.close
                                    cmd?volume    <- price.volume
                                    cmd?adj_close <- price.adjClose                                    
                                    yield { price with id = Some (cmd.ExecuteScalar<int64>()); securityId = sec.id } ] }      

  let get (id: int64) (from: DateTime option) =
    let days = if from.IsSome then int (DateTime.Now.Subtract(from.Value).TotalDays) else 1

    use db = new Db()
    // TODO: Can't just left join on share_platform, as a share will multiple ones, causing duplicates
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
    match (using (cmd.ExecuteReader()) (fun rdr ->
      if rdr.Read()
      then Some { id            = ofObj rdr?id
                  name          = rdr?name
                  currency      = None
                  previousName  = rdr?previous_name                
                  prices        = []
                  platforms     = Seq.empty<SecurityPlatform> }
      else None)) with
    | Some security -> 
      // 1st dispose previous reader before creating a new one
      Some { security with prices = getSharePriceByShare id days db }
    | _ -> None

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
                          prices        = List.empty<SecurityPrice>
                          platforms     = seq { yield { securityId  = Some rdr?id
                                                        platform    = enum rdr?platform_id
                                                        symbol      = rdr?symbol } } }
                else
                  None)

    match share with
    | Some ({ id = Some id' } as s) -> 
      Some { s with prices =  getSharePriceByShare id' 1 db }
    | _ -> None

// This is for the DB
// Integration.Saxo is for the Excel
module TransactionRepository =

  let x = 5