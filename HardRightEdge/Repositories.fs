module HardRightEdge.Repositories

open System
open System.Net
open System.Data.Common
open Option

open HardRightEdge.Domain
open HardRightEdge.Data
open HardRightEdge.Infrastructure.Common

module ShareRepository =

  let private saveDataProviderShare (share: DataProviderShare) =
    use db = new Db ()
    use cmd = db?Sql <- "UPDATE                 DataProviderShare
                        SET                     symbol = :symbol
                        WHERE                   shareId = :shareId
                        AND                     dataProviderId = :dataProviderId;

                        INSERT OR IGNORE INTO   DataProviderShare
                                                ( shareId,
                                                  dataProviderId,
                                                  symbol )
                        VALUES                  ( :shareId,
                                                  :dataProviderId,
                                                  :symbol )"
    db.Open()
    cmd?shareId <- share.shareId
    cmd?dataProviderId <- int share.dataProvider
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
                                      shareId,
                                      date,
                                      openp,
                                      high,
                                      low,
                                      close,
                                      volume,
                                      adjClose
                            FROM      SharePrice
                            WHERE     shareId = :shareId
                            AND       (:id IS NULL
                            OR        id < :id)
                            ORDER BY  id DESC
                            LIMIT     1"

        db.Open()
        cmd?shareId <- unwrap shareId
        cmd?id <- match sharePrice with
                  | Some { id = Some id' } -> unwrap id'
                  | _ -> null
        
        use rdr = cmd.ExecuteReader()
        if rdr.Read()
        then          
          let sharePrice = { id        = Some rdr?id;
                             shareId   = Some rdr?shareId;
                             date      = rdr?date;
                             openp     = rdr?openp;
                             high      = rdr?high;
                             low       = rdr?low;
                             close     = rdr?close;
                             adjClose  = rdr?adjClose;
                             volume    = rdr?volume }

          yield! getSharePriceByShare' shareId (Some sharePrice)
      }

    getSharePriceByShare' shareId None
   
  let private updateShare (share: Share) =
    use db = new Db ()
    use cmd = db?Sql <- "UPDATE Share
                          SET name = :name,
                              previousName = (SELECT name FROM Share WHERE id = 1 LIMIT 1)
                          where id = :id
                          and name <> :name;
                          
                          SELECT  previousName 
                          FROM    Share 
                          WHERE   id = :id;"

    db.Open ()
    cmd?name <- share.name
    cmd?id <- share.id
    let s = { share with previousName = ofObj(cmd.ExecuteScalar<string> ()) }
    s

  let private toShareViewName (name: string) = name.Replace (".", "_")

  let private createShareView name (id: int64 option) =
    match id with
    | Some idVal ->
      DbAdmin.createView (toShareViewName name)
                        (sprintf "SELECT  strftime('%%s', date) row_names,
                                          openp Open,
                                          high High,
                                          low Low,
                                          close Close,
                                          volume Volume,
                                          adjClose Adjusted
                                  FROM    SharePrice
                                  WHERE   shareId = %i" idVal) |> ignore
      id
     | _ -> id
    
  let insertShare (share: Share) =
    use db = new Db ()
    use cmd = db?Sql <- "INSERT INTO  Share
                                      ( name )
                        VALUES        ( :name );

                        SELECT CURRVAL(pg_get_serial_sequence('Share','id'));"
    db.Open ()
    cmd?name <- share.name
    Some (unbox<int64> (cmd.ExecuteScalar()))

  let save share =
    
    let shareId = match share with
                  | { id = Some sId } -> 
                    match updateShare share with
                    | { id = id'; name = nam; previousName = Some prevName } when prevName <> share.name -> 
                      // Name changed, therefore drop & recreate View
                      toShareViewName >> DbAdmin.dropView <| prevName |> ignore
                      createShareView nam id'
                    | _ -> share.id

                  | _ -> insertShare >> createShareView share.name <| share

    let savedShare = { share with 
                        id = shareId;
                        dataProviders = [| for dataProviderShare in share.dataProviders ->
                                            saveDataProviderShare { dataProviderShare with shareId = shareId } |] }
    use db = new Db ()
    use cmd = db?Sql <- "INSERT INTO  SharePrice 
                                      ( shareId, 
                                        date, 
                                        openp, 
                                        high, 
                                        low, 
                                        close, 
                                        volume, 
                                        adjClose  ) 
                          VALUES      ( :shareId, 
                                        :date, 
                                        :openp, 
                                        :high, 
                                        :low, 
                                        :close, 
                                        :volume, 
                                        :adjClose )"
    
    db.Open ()
    for price in share.prices do
      cmd?shareId <- shareId
      cmd?date <- price.date
      cmd?openp <- price.openp
      cmd?high <- price.high
      cmd?low <- price.low
      cmd?close <- price.close
      cmd?volume <- price.volume
      cmd?adjClose <- price.adjClose            
      cmd.ExecuteNonQuery() |> ignore
    
    savedShare

  let get (id: int64) =
    // TODO: Adapt the following code to work with multiple 
    // DataProviders, that will cause a Share to be returned 
    // for each DataProvider.

    use db = new Db()
    use cmd = db?Sql <- "SELECT     Share.id,
                                    name,
                                    previousName,
                                    dataProviderId,
                                    symbol
                          FROM      Share
                          LEFT JOIN DataProviderShare
                          ON        DataProviderShare.shareId = Share.id
                          WHERE     Share.id = :id
                          ORDER BY  Share.id"
    cmd?id <- id

    db.Open()
    use rdr = cmd.ExecuteReader()

    if rdr.Read()
    then Some { id = Some rdr?id
                name = rdr?name
                previousName = rdr?previousName
                // TODO: Call ShareRepository.getSharePriceByShare 1L |> Seq.take 1 |> Seq.toList
                // This will only include the latest share price.
                // The AppService can then fetch all new prices from this one until today
                // and save them.
                prices = [ (Seq.head (getSharePriceByShare id)) ]
                dataProviders = Seq.empty<DataProviderShare> }
    else None