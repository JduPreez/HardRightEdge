module HardRightEdge.Infrastructure

open System

module Exception =

  let unsupportedPatternRule () = failwith "Unsupported pattern rule"

module Common =

  open System.Globalization

  let ofObj<'t when 't: equality> value = if value = Unchecked.defaultof<'t> then None else Some value

  let ofNullable (value:System.Nullable<'T>) =  if value.HasValue then Some value.Value else None

  let toNullable option = match option with None -> System.Nullable() | Some v -> System.Nullable(v) 

  let toDbNull obj = match obj with null -> box DBNull.Value | _ -> obj

  let toDateTime pattern =
    let parse obj = DateTime.ParseExact(obj.ToString(), pattern.ToString(), null, DateTimeStyles.None)
    parse

  /// <remarks>
  /// Derived from http://stackoverflow.com/questions/6289761/how-to-downcast-from-obj-to-optionobj.
  /// </remarks>
  let unwrap (o: obj) =
    match o with
    | null -> null
    | _ ->
      let opTyp = typedefof<option<_>>

      let objTyp = o.GetType()
      let v = objTyp.GetProperty("Value")
      if objTyp.IsGenericType && objTyp.GetGenericTypeDefinition() = opTyp 
      then v.GetValue(o, [| |])
      else o

module Concurrent =

  open System.Threading.Tasks

  let inline awaitPlainTask (task: Task) = 
    // rethrow exception from preceding task if it fauled
    let continuation (t : Task) : unit =
        match t.IsFaulted with
        | true -> raise t.Exception
        | arg -> ()
    task.ContinueWith continuation |> Async.AwaitTask

module UnitOfWork =

  open System.Transactions

  type UnitOfWorkBuilder(isDurable) =
    let isDurable = isDurable
    
    member this.Bind(v, func) = Option.bind func v

    member this.Return(v) = Some v

    member this.ReturnFrom(v) = v

    member this.Zero() = this.Return ()

    member this.Delay(func) = func

    member this.Run(func) =
      let txOptions = new TransactionOptions()
      txOptions.IsolationLevel = IsolationLevel.ReadUncommitted |> ignore

      use txScope = new TransactionScope(TransactionScopeOption.Required, txOptions)
      let result = func()

      if isDurable
      then txScope.Complete()

      result

  let durable = new UnitOfWorkBuilder(true)
  let temp = new UnitOfWorkBuilder(false)

module FileSystem =

  open System.IO

  let (+/) path1 path2 = Path.Combine(path1, path2)

  let isDir path = (File.GetAttributes(path) ||| FileAttributes.Directory) = FileAttributes.Directory

  let files path searchPattern =  if isDir path
                                  then Directory.EnumerateFiles(path, searchPattern)
                                  else List.empty<string> :> seq<string>

module Serialization =

  open Microsoft.FSharpLu.Json
  
  let toJson = Compact.serialize

  let inline fromJson json = Compact.deserialize json