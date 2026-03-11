# Test Plan

## Scope
- Order lifecycle
- Inventory serve & release
- Payment intent & confirmation
- Rollback when payment fail
- Idempotency behavior

## Test Levels
- Unit Test => Service Test
- Integration Test => Test flow O-I-P
- Regression Test => Logic test
- Negative Test => Error & edge cases

## Test Types
- Functional Testing
- Concurrency Testing
- Transaction Testing
- Security Testing (basic)

## Test Environment
- OS: Window, Docker
- Backend: Flask
- DB: PostgreSQL
- Cache: Redis
- Tools: Pytest