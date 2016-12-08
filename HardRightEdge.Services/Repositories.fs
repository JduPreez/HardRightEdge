namespace HardRightEdge.Services.Repositories

open System
open System.Net
open System.Data.Common

open HardRightEdge.Services.Domain
open HardRightEdge.Services.Data

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
        cmd?stockId <- stockId
        cmd?id <- match stockPrice with
                  | Some { id = Some id' } -> Nullable<int64>(id')
                  | _ -> Nullable<int64>()
        
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
                        SET     name = :name
                        WHERE   id = :id"

    db.Open ()
    cmd?name <- stock.name
    cmd?id <- stock.id
    cmd.ExecuteNonQuery () |> ignore
    stock.id

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
                  | { id = Some sId } -> updateStock stock
                  | _ -> insertStock stock   

    { stock with 
        id = stockId;
        dataProviders = [| for dataProviderStock in stock.dataProviders ->
                              saveDataProviderStock { dataProviderStock with stockId = stockId } |] }


      (*use db = new Db ()
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
          cmd?stockId <- stock.id
          cmd?date <- price.date
          cmd?openp <- price.openp
          cmd?high <- price.high
          cmd?low <- price.low
          cmd?close <- price.close
          cmd?volume <- price.volume
          cmd?adjClose <- price.adjClose            
          cmd.ExecuteNonQuery() |> ignore
      None*)

  let get (id: int64) =
    // TODO: Adapt the following code to work with multiple 
    // DataProviders, that will cause a Stock to be returned 
    // for each DataProvider.

    use db = new Db()
    use cmd = db?Sql <- "SELECT     Stock.id,
                                    name,
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
                // TODO: Call StockRepository.getStockPriceByStock 1L |> Seq.take 1 |> Seq.toList
                // This will only include the latest stock price.
                // The AppService can then fetch all new prices from this one until today
                // and save them.
                prices = [ (Seq.head (getStockPriceByStock id)) ]
                dataProviders = Seq.empty<DataProviderStock> }
    else None