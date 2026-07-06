# 0002. Deterministic seeded RNG

- **Status:** accepted
- **Date:** 2026-07-06 09:57:52

## Context

Dice, hyperspace warps and mystery cards are random; fairness and reproducibility matter for a kids' game and for tests.

## Decision

Randomness goes through an IRandom interface with a seedable default implementation, injected into the engine.

## Rationale

Seeded games are replayable and unit-testable; fairness properties can be asserted.

## Alternatives Considered

- Static System.Random calls

## Consequences

All random behavior must flow through the injected source.

