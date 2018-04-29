module HardRightEdge.Data

open System
open System.IO
open System.Net
open System.Data
open System.Data.Common
open Npgsql
open HardRightEdge.Infrastructure.Common
open Microsoft.FSharp.Core.Operators.Unchecked

type DynamicDbDataReader (reader:NpgsqlDataReader) =

  member private me.Reader = reader
  member me.Read() = reader.Read()

  static member (?) (dr:DynamicDbDataReader, name:string) : 'R =
    let colIndex = dr.Reader.GetOrdinal(name)
    
    if dr.Reader.IsDBNull(colIndex)
    then defaultof<'R>
    else unbox (dr.Reader.[name])

  member me.HasRows
    with get () = reader.HasRows

  interface IDisposable with
      member me.Dispose() = reader.Dispose()

type DynamicDbCommand (cmd:NpgsqlCommand) = 
  member private me.Command = cmd
      
  static member (?<-) (cmd:DynamicDbCommand, name:string, value: obj) = 
    // TODO: Take the pattern match that converts value:obj to null for
    // option types, and put into a separate function. Then call this from 
    // the "else ..." part too.
    if cmd.Command.Parameters.Contains(name) then        
      cmd.Command.Parameters.RemoveAt(name)

    cmd.Command.Parameters.Add(NpgsqlParameter(name, unwrap value |> toDbNull)) |> ignore       

  member me.ExecuteNonQuery() = cmd.ExecuteNonQuery()

  member me.ExecuteReader() = new DynamicDbDataReader(cmd.ExecuteReader())

  member me.ExecuteScalar<'t>() = cmd.ExecuteScalar() :?> 't

  member me.Parameters = cmd.Parameters

  interface IDisposable with
      member me.Dispose() = cmd.Dispose()

type Db (conn: NpgsqlConnection) =       

  static member Name = "hard_right_edge"

  member private me.Connection = conn

  static member (?<-) (conn:Db, name, query) =
      let cmd = new NpgsqlCommand(query, conn.Connection)
      cmd.CommandType <- CommandType.Text
      new DynamicDbCommand(cmd)

  static member (?) (conn:Db, sproc) = 
      let cmd = new NpgsqlCommand(sproc, conn.Connection)
      cmd.CommandType <- CommandType.StoredProcedure
      new DynamicDbCommand(cmd)

  member me.Open() = 
    conn.Open()

  new () = new Db(Db.Name)

  new (dbName: string) = new Db(new NpgsqlConnection(sprintf "Host=localhost;Port=5432;Username=hard_right_edge_app;Password=p@ssword123;Database=%s;Enlist=true" Db.Name))
  
  interface IDisposable with
      member me.Dispose() = conn.Dispose()

module DbAdmin =

  let dropView name =
    use db = new Db()
    use cmd = db?Sql <- sprintf "DROP VIEW IF EXISTS %O" (unwrap name)
    db.Open()
    cmd.ExecuteNonQuery() |> ignore

  let createView name query = 
    use db = new Db()
    use cmd = db?Sql <- sprintf "CREATE VIEW %s 
                                AS
                                %s" name query

    db.Open ()
    cmd.ExecuteNonQuery () |> ignore

  // TODO: Rewrite this for Postgresql
  (*let prepare dbName =
    if not (File.Exists dbName)
    then NpgsqlConnection.CreateFile (Db.FileName dbName)

    use db = new Db()
    use cmd = db?Sql <- "SELECT name FROM sqlite_master WHERE type='table' AND name='StockPrice'"

    db.Open()
    use rdr = cmd.ExecuteReader()
    if not rdr.HasRows
    then
      // Table Stock
      use createTblStock = db?Sql <- "CREATE TABLE Stock (id INTEGER PRIMARY KEY AUTOINCREMENT, 
                                                          name TEXT NOT NULL,
                                                          previousName TEXT)"
      createTblStock.ExecuteNonQuery() |> ignore

      // Table StockPrice
      use createTblStockPrice = db?Sql <- "CREATE TABLE StockPrice 
                                                        ( id INTEGER PRIMARY KEY AUTOINCREMENT, 
                                                          stockId INTEGER NOT NULL, 
                                                          date DATETIME NOT NULL, 
                                                          openp NUMERIC, 
                                                          high NUMERIC, 
                                                          low NUMERIC, 
                                                          close NUMERIC, 
                                                          volume NUMERIC, 
                                                          adjClose NUMERIC,
                                                          FOREIGN KEY (stockId) REFERENCES Stock(id) )"
      createTblStockPrice.ExecuteNonQuery() |> ignore

      // Table DataProviderStock
      use createTblDataProviderStock = db?Sql <- "CREATE TABLE  DataProviderStock 
                                                                ( stockId INT NOT NULL, 
                                                                  dataProviderId INT NOT NULL, 
                                                                  symbol TEXT NOT NULL, 
                                                                  PRIMARY KEY (stockId, dataProviderId),
                                                                  FOREIGN KEY (stockId) REFERENCES Stock(id) )"
      createTblDataProviderStock.ExecuteNonQuery() |> ignore*)

