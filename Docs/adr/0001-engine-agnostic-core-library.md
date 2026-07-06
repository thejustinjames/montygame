# 0001. Engine-agnostic core library

- **Status:** accepted
- **Date:** 2026-07-06 09:57:52

## Context

The locked tech stack is Unity, but Unity cannot be exercised by automated validation without a licensed editor in batchmode.

## Decision

All game logic lives in MontyGame.Core, a pure .NET 8 class library with zero UnityEngine references; Unity gets thin MonoBehaviour shells that only call the core API.

## Rationale

Maximizes the logic that agents and CI can build, test and verify; minimizes untestable editor wiring; standard pattern for testable Unity games.

## Alternatives Considered

- All-in Unity C# scripts (untestable without the editor)
- Web prototype (throwaway for the real game)

## Consequences

Unity Sprint 1 becomes integration work; the core API must stay engine-neutral (no UnityEngine types in public signatures).

