namespace HardRightEdge.Services

open System

module Infrastructure =
  
  module Exception =

    let unsupportedPatternRule () = failwith "Unsupported pattern rule"

  module Common =
    // http://stackoverflow.com/questions/6289761/how-to-downcast-from-obj-to-optionobj
    let nullIfNone (value: obj) =
      if value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() = typedefof<option<_>>
      then  
        

      else value