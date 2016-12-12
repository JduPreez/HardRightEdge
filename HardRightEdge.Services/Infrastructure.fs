namespace HardRightEdge.Services

open System

module Infrastructure =
  
  module Exception =

    let unsupportedPatternRule () = failwith "Unsupported pattern rule"

  module Common =

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