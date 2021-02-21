﻿module FSharpx.Tests.StateTest

open FSharpx
open FSharpx.State
open NUnit.Framework
open FsUnitTyped
open TestHelpers

// Simple example
let tick = state {
  let! n = getState
  do! putState (n + 1)
  return n }

[<Test>]
let ``When starting a ticker at 0, it should have a state of 0``() =
  let actual = tick 0
  fst actual |> shouldEqual 0

[<Test>]
let ``When starting a ticker at 0 and ticking twice, it should have a state of 2``() =
  let test = state {
    let! _ = tick
    let! _ = tick
    return () }
  let actual = exec test 0
  actual |> shouldEqual 2

// Stack example
let enqueue a = fun s -> ((), s @ a::[])
let dequeue = function (hd::tl) -> (hd, tl) | [] -> failwith "dequeue"

let workflow = state {
  let! queue = getState
  do! enqueue 4
  let! hd = dequeue
  do! enqueue (hd * 3)
  return hd }

[<Test>]
let ``When running the workflow, it should return 4``() =
  eval workflow [] |> shouldEqual 4

[<Test>]
let ``use should dispose underlying IDisposable``() =
  let disposeChecker = new DisposeChecker()
  let r =
     state {
       use! x = state { return disposeChecker }
       return x.Disposed
     }
  Assert.Multiple
    (fun () ->
      eval r () |> shouldEqual false
      disposeChecker.Disposed |> shouldEqual true
    )
