module HardRightEdge.Infrastructure

open System

module Exception =

  let unsupportedPatternRule () = failwith "Unsupported pattern rule"

module Common =

  let ofObj value = match value with null -> None | _ -> Some value

  let ofNullable (value:System.Nullable<'T>) =  if value.HasValue then Some value.Value else None

  let toNullable option = match option with None -> System.Nullable() | Some v -> System.Nullable(v) 

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
      if objTyp.IsGenericType && objTyp.GetGenericTypeDefinition() = opTyp then
        if o = null then null
        else v.GetValue(o, [| |])
      else o

module Concurrent =

  open System.Threading
  open System.Threading.Tasks

  let inline awaitPlainTask (task: Task) = 
    // rethrow exception from preceding task if it fauled
    let continuation (t : Task) : unit =
        match t.IsFaulted with
        | true -> raise t.Exception
        | arg -> ()
    task.ContinueWith continuation |> Async.AwaitTask