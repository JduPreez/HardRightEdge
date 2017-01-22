﻿namespace HardRightEdge.Services

open System
open System.IO
open System.Net
open System.Data
open System.Data.Common
open System.Data.SQLite
open HardRightEdge.Services.Infrastructure.Common

module Data = 

  type DynamicDbDataReader (reader:SQLiteDataReader) =

    member private me.Reader = reader
    member me.Read() = reader.Read()
    static member (?) (dr:DynamicDbDataReader, name:string) : 'R =
      unbox (dr.Reader.[name])

    member me.HasRows
      with get () = reader.HasRows

    interface IDisposable with
        member me.Dispose() = reader.Dispose()

  type DynamicDbCommand (cmd:SQLiteCommand) = 
    member private me.Command = cmd
        
    static member (?<-) (cmd:DynamicDbCommand, name:string, value: obj) = 
      // TODO: Take the pattern match that converts value:obj to null for
      // option types, and put into a separate function. Then call this from 
      // the "else ..." part too.
      if cmd.Command.Parameters.Contains(name) then        
        cmd.Command.Parameters.RemoveAt(name)
      
      cmd.Command.Parameters.Add(SQLiteParameter(name, unwrap value)) |> ignore       

    member me.ExecuteNonQuery() = cmd.ExecuteNonQuery()

    member me.ExecuteReader() = new DynamicDbDataReader(cmd.ExecuteReader())

    member me.ExecuteScalar<'t>() = cmd.ExecuteScalar() :?> 't

    member me.Parameters = cmd.Parameters

    interface IDisposable with
        member me.Dispose() = cmd.Dispose()

  type Db (conn: SQLiteConnection) =       

    static member FileName dbName = sprintf "%s.db" dbName

    static member Name = "HardRightEdge"

    member private me.Connection = conn

    static member (?<-) (conn:Db, name, query) =
        let cmd = new SQLiteCommand(query, conn.Connection)
        cmd.CommandType <- CommandType.Text
        new DynamicDbCommand(cmd)

    static member (?) (conn:Db, sproc) = 
        let cmd = new SQLiteCommand(sproc, conn.Connection)
        cmd.CommandType <- CommandType.StoredProcedure
        new DynamicDbCommand(cmd)
  
    member me.Open() = 
      conn.Open()

    new () = new Db(Db.Name)

    new (dbName: string) = new Db(new SQLiteConnection(sprintf "Data Source=%s;Version=3;" (Db.FileName dbName)))
    
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

    let prepare dbName =
      if not (File.Exists dbName)
      then SQLiteConnection.CreateFile (Db.FileName dbName)

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
        createTblDataProviderStock.ExecuteNonQuery() |> ignore
