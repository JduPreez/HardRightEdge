module HardRightEdge.Repositories

open System
open Microsoft.FSharp.Core.Operators.Unchecked

open HardRightEdge.Domain
open HardRightEdge.Data
open HardRightEdge.Infrastructure.Common

module Securities =
    
  let private saveSecurityPlatform (securityPlatform: SecurityPlatform) =
    use db = new Db ()
    use cmd = db?Sql <- "INSERT INTO    security_platform
                                        ( security_id,
                                          platform_id,
                                          symbol )
                        VALUES          ( :security_id,
                                          :platform_id,
                                          :symbol )
                        ON CONFLICT     ( symbol,
                                          platform_id) 
                        DO UPDATE          
                        SET security_id = :security_id"
    db.Open()
    cmd?security_id         <- securityPlatform.securityId
    cmd?platform_id         <- int securityPlatform.platform
    cmd?symbol              <- securityPlatform.symbol
    cmd.ExecuteNonQuery ()  |> ignore
    securityPlatform

  let private getSecurityPriceBySecurity (securityId: int64) (days: int) (db: Db) =
    use cmd = db?Sql <- (sprintf "SELECT    id,
                                            security_id,
                                            date,
                                            openp,
                                            high,
                                            low,
                                            close,
                                            volume,
                                            adj_close
                                  FROM      security_price
                                  WHERE     security_id = :security_id
                                  ORDER BY  id DESC                                  
                                  LIMIT     %i" days)

    if db.IsClosed then db.Open()
    
    cmd?security_id <- securityId
    
    use rdr = cmd.ExecuteReader()
    [ while rdr.Read() do      
        yield { id          = ofObj rdr?id
                securityId  = ofObj rdr?security_id
                date        = rdr?date
                openp       = rdr?openp
                high        = rdr?high
                low         = rdr?low
                close       = rdr?close
                adjClose    = ofObj rdr?adj_close
                volume      = rdr?volume } ]

  let private getSecurityPriceBySecurity' (securityId: int64) (days: int) =
    use db = new Db()
    getSecurityPriceBySecurity securityId days db

  let update (security: Security) =
    use db = new Db ()
    use cmd = db?Sql <- "UPDATE security
                          SET   name = :name,
                                previous_name = :previous_name
                          WHERE id = :id"

    db.Open ()
    cmd?name          <- security.name
    cmd?id            <- security.id
    cmd?previous_name <- security.previousName
    cmd.ExecuteNonQuery () |> ignore
    security
    
  let insert (security: Security) =
    use db = new Db ()
    use cmd = db?Sql <- "INSERT INTO  security
                                      ( name )
                        VALUES        ( :name );

                        SELECT CURRVAL(pg_get_serial_sequence('security','id'));"
    db.Open ()
    cmd?name <- security.name
    { security with id = Some (unbox<int64> (cmd.ExecuteScalar())) }

  let save (security: Security) =
    let sec = match security with
              | { id = Some _ } -> update security
              | _               -> insert security

    let savedSec = {  sec with                        
                        platforms = [| for securityPlatform in security.platforms ->
                                        saveSecurityPlatform { securityPlatform with securityId = sec.id } |] }
    use db = new Db ()
    use cmd = db?Sql <- "INSERT INTO  security_price 
                                      ( security_id, 
                                        date, 
                                        openp, 
                                        high, 
                                        low, 
                                        close, 
                                        volume, 
                                        adj_close  ) 
                          VALUES      ( :security_id, 
                                        :date, 
                                        :openp, 
                                        :high, 
                                        :low, 
                                        :close, 
                                        :volume, 
                                        :adj_close );

                        SELECT CURRVAL(pg_get_serial_sequence('security_price','id'));"
    
    db.Open ()
    { savedSec with prices = [  for price in security.prices do
                                    cmd?security_id <- sec.id
                                    cmd?date        <- price.date
                                    cmd?openp       <- price.openp
                                    cmd?high        <- price.high
                                    cmd?low         <- price.low
                                    cmd?close       <- price.close
                                    cmd?volume      <- price.volume
                                    cmd?adj_close   <- price.adjClose                                    
                                    yield { price with id = Some (cmd.ExecuteScalar<int64>()); securityId = sec.id } ] }      

  let getById (id: int64) (from: DateTime option) =
    let days = if from.IsSome then int (DateTime.Now.Subtract(from.Value).TotalDays) else 1

    use db = new Db()
    // TODO: Can't just left join on security_platform, as a security will multiple platforms will return duplicates
    use cmd = db?Sql <- "SELECT     security.id,
                                    name,
                                    previous_name,
                                    platform_id,
                                    symbol
                          FROM      security

                          LEFT JOIN security_platform
                          ON        security_platform.security_id = security.id
                          
                          WHERE     security.id = :id
                          ORDER BY  security.id"
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
      Some { security with prices = getSecurityPriceBySecurity id days db }
    | _ -> None

  let getBySymbol (symbol: string) (platform: Platform) =
    use db = new Db()
    use cmd = db?Sql <- "SELECT       security.id,
                                      security.name,
                                      security.previous_name,
                                      security_platform.platform_id,
                                      security_platform.symbol

                          FROM        security

                          INNER JOIN  security_platform
                          ON          security_platform.security_id = security.id

                          WHERE       security_platform.symbol = :symbol
                          AND         security_platform.platform_id = :platform_id

                          LIMIT 1"
    cmd?symbol      <- symbol
    cmd?platform_id <- int platform

    db.Open()
    let sec = using (cmd.ExecuteReader()) (fun rdr ->
                if rdr.Read()
                then
                  Some {  id            = Some rdr?id
                          name          = rdr?name
                          currency      = None
                          previousName  = rdr?previous_name
                          prices        = []
                          platforms     = seq { yield { securityId  = Some rdr?id
                                                        platform    = enum rdr?platform_id
                                                        symbol      = rdr?symbol } } }
                else
                  None)

    match sec with
    | Some ({ id = Some id' } as s) -> 
      Some { s with prices =  getSecurityPriceBySecurity id' 1 db }
    | _ -> None

  let get (from: DateTime option) =
    let days = if from.IsSome then int (DateTime.Now.Subtract(from.Value).TotalDays) else 1
    
    use db = new Db()
    use cmd = db?Sql <- "SELECT     security.id,
                                    security.name,
                                    security.previous_name,
                                    security_platform.platform_id,
                                    security_platform.symbol

                          FROM      security
    
                          LEFT JOIN security_platform
                          ON        security_platform.security_id = security.id
                          
                          ORDER BY  security.id"
    db.Open()

    let securities = using (cmd.ExecuteReader()) (fun rdr ->
                      [while rdr.Read() do
                        yield { id            = ofObj rdr?id
                                name          = rdr?name
                                currency      = None
                                previousName  = rdr?previous_name                
                                prices        = []
                                platforms     = seq { yield { securityId = ofObj rdr?id
                                                              platform = enum rdr?platform_id
                                                              symbol = rdr?symbol } } }]
                        |> List.groupBy (fun sec -> sec.id)
                        |> List.map (fun (_, secs) -> 
                                      { secs.Head with
                                          platforms = Seq.collect (fun s -> s.platforms) secs }))

    [for sec in securities -> 
      { sec with prices = getSecurityPriceBySecurity sec.id.Value days db }]

// This is for the DB
// Integration.Saxo is for the Excel
module TransactionRepository =

  let x = 5