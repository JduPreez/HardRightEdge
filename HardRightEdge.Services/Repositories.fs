namespace HardRightEdge.Services.Repositories

open System
open System.Net
open System.Data.Common
open Option

open HardRightEdge.Services.Domain
open HardRightEdge.Services.Data
open HardRightEdge.Services.Infrastructure.Common

module StockRepository =

  let private saveDataProviderStock (stock: DataProviderStock) =
    use db = new Db ()
    use cmd = db?Sql <- "UPDATE                 DataProviderStock
                        SET                     symbol = :symbol
                        WHERE                   stockId = :stockId
                        AND                     dataProviderId = :dataProviderId;

                        INSERT OR IGNORE INTO   DataProviderStock
                                                ( stockId,
                                                  dataProviderId,
                                                  symbol )
                        VALUES                  ( :stockId,
                                                  :dataProviderId,
                                                  :symbol )"
    db.Open()
    cmd?stockId <- stock.stockId
    cmd?dataProviderId <- int stock.dataProvider
    cmd?symbol <- stock.symbol
    cmd.ExecuteNonQuery () |> ignore
    stock

  let private getStockPriceByStock (stockId: int64) = 
    let rec getStockPriceByStock' (stockId: int64) (stockPrice: StockPrice option) =
      seq {
        if stockPrice.IsSome
        then yield stockPrice.Value

        use db = new Db()
        use cmd = db?Sql <- "SELECT   id,
                                      stockId,
                                      date,
                                      openp,
                                      high,
                                      low,
                                      close,
                                      volume,
                                      adjClose
                            FROM      StockPrice
                            WHERE     stockId = :stockId
                            AND       (:id IS NULL
                            OR        id < :id)
                            ORDER BY  id DESC
                            LIMIT     1"

        db.Open()
        cmd?stockId <- unwrap stockId
        cmd?id <- match stockPrice with
                  | Some { id = Some id' } -> unwrap id'
                  | _ -> null
        
        use rdr = cmd.ExecuteReader()
        if rdr.Read()
        then          
          let stockPrice = { id        = Some rdr?id;
                             stockId   = Some rdr?stockId;
                             date      = rdr?date;
                             openp     = rdr?openp;
                             high      = rdr?high;
                             low       = rdr?low;
                             close     = rdr?close;
                             adjClose  = rdr?adjClose;
                             volume    = rdr?volume }

          yield! getStockPriceByStock' stockId (Some stockPrice)
      }

    getStockPriceByStock' stockId None
   
  let private updateStock (stock: Stock) =
    use db = new Db ()
    use cmd = db?Sql <- "UPDATE Stock
                          SET name = :name,
                              previousName = (SELECT name FROM Stock WHERE id = 1 LIMIT 1)
                          where id = :id
                          and name <> :name;
                          
                          SELECT  previousName 
                          FROM    Stock 
                          WHERE   id = :id;"

    db.Open ()
    cmd?name <- stock.name
    cmd?id <- stock.id
    let s = { stock with previousName = ofObj(cmd.ExecuteScalar<string> ()) }
    s

  let private toStockViewName (name: string) = name.Replace (".", "_")

  let private createStockView name (id: int64 option) =
    match id with
    | Some idVal ->
      DbAdmin.createView (toStockViewName name)
                        (sprintf "SELECT  strftime('%%s', date) row_names,
                                          openp Open,
                                          high High,
                                          low Low,
                                          close Close,
                                          volume Volume,
                                          adjClose Adjusted
                                  FROM    StockPrice
                                  WHERE   stockId = %i" idVal) |> ignore
      id
     | _ -> id
    

  let private insertStock (stock: Stock) =
    use db = new Db ()
    use cmd = db?Sql <- "INSERT INTO  Stock
                                      ( name )
                        VALUES        ( :name );

                        SELECT last_insert_rowid();"
    db.Open ()
    cmd?name <- stock.name
    Some (unbox<int64> (cmd.ExecuteScalar()))

  let save stock =
    
    let stockId = match stock with
                  | { id = Some sId } -> 
                    match updateStock stock with
                    | { id = id'; name = nam; previousName = Some prevName } when prevName <> stock.name -> 
                      // Name changed, therefore drop & recreate View
                      toStockViewName >> DbAdmin.dropView <| prevName |> ignore
                      createStockView nam id'
                    | _ -> stock.id

                  | _ -> insertStock >> createStockView stock.name <| stock

    let savedStock = { stock with 
                        id = stockId;
                        dataProviders = [| for dataProviderStock in stock.dataProviders ->
                                            saveDataProviderStock { dataProviderStock with stockId = stockId } |] }
    use db = new Db ()
    use cmd = db?Sql <- "INSERT INTO  StockPrice 
                                      ( stockId, 
                                        date, 
                                        openp, 
                                        high, 
                                        low, 
                                        close, 
                                        volume, 
                                        adjClose  ) 
                          VALUES      ( :stockId, 
                                        :date, 
                                        :openp, 
                                        :high, 
                                        :low, 
                                        :close, 
                                        :volume, 
                                        :adjClose )"
    
    db.Open ()
    for price in stock.prices do
      cmd?stockId <- stockId
      cmd?date <- price.date
      cmd?openp <- price.openp
      cmd?high <- price.high
      cmd?low <- price.low
      cmd?close <- price.close
      cmd?volume <- price.volume
      cmd?adjClose <- price.adjClose            
      cmd.ExecuteNonQuery() |> ignore
    
    savedStock

  let get (id: int64) =
    // TODO: Adapt the following code to work with multiple 
    // DataProviders, that will cause a Stock to be returned 
    // for each DataProvider.

    use db = new Db()
    use cmd = db?Sql <- "SELECT     Stock.id,
                                    name,
                                    previousName,
                                    dataProviderId,
                                    symbol
                          FROM      Stock
                          LEFT JOIN DataProviderStock
                          ON        DataProviderStock.stockId = Stock.id
                          WHERE     Stock.id = :id
                          ORDER BY  Stock.id"
    cmd?id <- id

    db.Open()
    use rdr = cmd.ExecuteReader()

    if rdr.Read()
    then Some { id = Some rdr?id
                name = rdr?name
                previousName = rdr?previousName
                // TODO: Call StockRepository.getStockPriceByStock 1L |> Seq.take 1 |> Seq.toList
                // This will only include the latest stock price.
                // The AppService can then fetch all new prices from this one until today
                // and save them.
                prices = [ (Seq.head (getStockPriceByStock id)) ]
                dataProviders = Seq.empty<DataProviderStock> }
    else None