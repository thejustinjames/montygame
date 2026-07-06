# 0003. .NET 8 LTS with xunit

- **Status:** accepted
- **Date:** 2026-07-06 09:57:52

## Context

The core library needs a long-lived, Unity-compatible C# target and a test framework.

## Decision

Target net8.0 (netstandard2.1-compatible API surface where practical) and use xunit for tests.

## Rationale

LTS support; xunit is the de-facto .NET test framework; Unity 2022+ consumes modern C# libraries.

## Alternatives Considered

- net48
- NUnit

## Consequences

Avoid APIs newer than what the Unity scripting runtime consumes.

