namespace HardRightEdge

module Presentation =

  open System
  open System.Collections.Generic
  open FSharp.Control.Reactive
  open FSharp.Control.Reactive.Observable
  
  module Controller =

    // TODO: Test if the change to Map<..., ...> still works
    let private observations : Map<string, IDisposable> ref = ref Map.empty<string, IDisposable>

    let private subscribe<'t> modls (onNext : 't -> unit) =         
      let typeAsKey = typeof<'t>.FullName
      let obs = !observations
      let onNextForT m = onNext(unbox m)
      match obs.TryFind typeAsKey with
      | None -> observations := obs.Add (typeAsKey, modls |> subscribeWithCallbacks onNextForT ignore ignore)                                                      
                ()
      | _ -> ()

    /// <summary> Subscribe to model observables. </summary>
    let show1<'model> (scene: unit -> ('model -> unit)) (modls : IObservable<_>) =
      let observer = scene ()
      subscribe<'model> modls observer

    /// <summary> Subscribe to model observables, and return the View so that the caller can do something
    // with it, like execute Application.Run(view) for Windows Forms. </summary>
    let show2<'view, 'model> (scene: unit -> ('view * ('model -> unit))) (modls : IObservable<_>) =
      let view, observer = scene ()      
      subscribe<'model> modls observer
      view


  // TODO: Test this module!
  module Model =

    let private models : Map<string, IObservable<_>> ref = ref Map.empty<string, IObservable<_>>

    let get<'t> () : seq<'t> option = 
      let typeAsKey = (typeof<'t>.FullName)
      let mdls = !models
      match mdls.TryFind typeAsKey with
      | Some ms -> ms :?> ResizeArray<_> |> Seq.cast |> Some
      | None -> None

    let with' (modls : seq<'t>) =
      let typeAsKey = (typeof<'t>.FullName)
      let derefMs = !models

      match derefMs.TryFind typeAsKey with
      | None ->     
        let obs = modls
                  |> Seq.map (fun m -> box m)
                  |> ResizeArray // Use mutable CLR list, instead of F# immutable list
                  |> toObservable
        models := derefMs.Add (typeAsKey, obs)
        obs        

      | Some obs -> 
        let items = obs :?> ResizeArray<_> // We know the underlying is really a list
        items.AddRange(modls)
        obs