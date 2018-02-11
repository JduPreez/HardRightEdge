module HardRightEdge.Repositories

open System
open System.Net
open System.Data.Common
open Option
open Microsoft.FSharp.Core.Operators.Unchecked

open HardRightEdge.Domain
open HardRightEdge.Data
open HardRightEdge.Infrastructure.Common

module ShareRepository =
    
  let private saveSharePlatform (share: SharePlatform) =
    use db = new Db ()
    use cmd = db?Sql <- "UPDATE                 SharePlatform
                        SET                     symbol = :symbol
                        WHERE                   share_id = :share_id
                        AND                     platform_id = :platform_id;

                        INSERT OR IGNORE INTO   SharePlatform
                                                ( share_id,
                                                  platform_id,
                                                  symbol )
                        VALUES                  ( :share_id,
                                                  :platform_id,
                                                  :symbol )"
    db.Open()
    cmd?share_id <- share.shareId
    cmd?platform_id <- int share.platform
    cmd?symbol <- share.symbol
    cmd.ExecuteNonQuery () |> ignore
    share

  let private getSharePriceByShare (shareId: int64) = 
    let rec getSharePriceByShare' (shareId: int64) (sharePrice: SharePrice option) =
      seq {
        if sharePrice.IsSome
        then yield sharePrice.Value

        use db = new Db()
        use cmd = db?Sql <- "SELECT   id,
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
                            AND       (:id IS NULL
                            OR        id < :id)
                            ORDER BY  id DESC
                            LIMIT     1"

        db.Open()
        cmd?share_id <- unwrap shareId
        cmd?id <- match sharePrice with
                  | Some { id = Some id' } -> unwrap id'
                  | _ -> null
        
        use rdr = cmd.ExecuteReader()
        if rdr.Read()
        then          
          let sharePrice = { id        = Some rdr?id;
                             shareId   = Some rdr?share_id;
                             date      = rdr?date;
                             openp     = rdr?openp;
                             high      = rdr?high;
                             low       = rdr?low;
                             close     = rdr?close;
                             adjClose  = rdr?adj_close;
                             volume    = rdr?volume }

          yield! getSharePriceByShare' shareId (Some sharePrice)
      }

    getSharePriceByShare' shareId None
   
  let updateShare (share: Share) =
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
    
  let insertShare (share: Share) =
    use db = new Db ()
    use cmd = db?Sql <- "INSERT INTO  share
                                      ( name )
                        VALUES        ( :name );

                        SELECT CURRVAL(pg_get_serial_sequence('Share','id'));"
    db.Open ()
    cmd?name <- share.name
    { share with id = Some (unbox<int64> (cmd.ExecuteScalar())) }

  let saveShare share =
    let shr = match share with
              | { id = Some sId } -> updateShare share
              | _                 -> insertShare share

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
                                        :adj_close )"
    
    db.Open ()
    for price in share.prices do
      cmd?share_id <- shr.id
      cmd?date <- price.date
      cmd?openp <- price.openp
      cmd?high <- price.high
      cmd?low <- price.low
      cmd?close <- price.close
      cmd?volume <- price.volume
      cmd?adj_close <- price.adjClose            
      cmd.ExecuteNonQuery() |> ignore
    
    savedShare

  let getShare (id: int64) =
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
    then Some { id = Some rdr?id
                name = rdr?name
                previousName = rdr?previous_name
                // TODO: Call ShareRepository.getSharePriceByShare 1L |> Seq.take 1 |> Seq.toList
                // This will only include the latest share price.
                // The AppService can then fetch all new prices from this one until today
                // and save them.
                prices = [] // [ (Seq.head (getSharePriceByShare id)) ]
                platforms = Seq.empty<SharePlatform> }
    else None