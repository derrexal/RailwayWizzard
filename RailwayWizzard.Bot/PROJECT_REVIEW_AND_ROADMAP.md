# Project Review And Roadmap

## Findings

1. `10/10` Global state in notification dialogs can leak between users and break parallel conversations.
2. `9/10` `/api/sendMessageForUser` has no authentication and can be abused by anyone who can reach it.
3. `9/10` `robot_queries.py` returns `aiohttp.ClientResponse` objects after leaving `async with`, which is fragile.
4. `8/10` The project mixes multiple Telegram SDKs: `python-telegram-bot`, `telebot`, `pyTelegramBotAPI`, and `telegram`.
5. `8/10` `404` handling in `robot_queries.py` is effectively unreachable because `raise_for_status()` already raises earlier.
6. `8/10` There is a race between HTTP server startup and Telegram bot loop initialization.
7. `8/10` `python-telegram-bot==20.0a0` is an old alpha dependency and already caused API compatibility issues.
8. `7/10` Conversation state is stored under magic numeric keys in `context.user_data`.
9. `7/10` `ConversationHandler` warnings were suppressed instead of resolved structurally.
10. `7/10` External HTTP calls have no explicit timeouts or retry policy.
11. `6/10` Some conversation handlers do not consistently return state transitions in error paths.
12. `6/10` The HTTP server is published on a random host port in Docker Compose.
13. `6/10` FastAPI request bodies are untyped and skip Pydantic validation.
14. `5/10` Broad `except Exception` blocks hide the difference between transport, validation, and logic errors.
15. `5/10` Several config values are hardcoded in source instead of a single config layer.
16. `4/10` `NotificationTask.CarTypes` is typed as `List[str]` while integer enum codes are actually passed.
17. `4/10` Some user-facing URLs miss an explicit `https://` scheme.
18. `3/10` Date parsing is rigid and may surprise users around year boundaries.

## Roadmap

### Session 1

1. Remove global shared state from notification flows.
   - Move `popular_cities`, `car_types`, and `car_type_markup` to per-user state.
2. Replace numeric keys in `context.user_data`.
   - Use named fields or a state object instead of `0`, `1`, `22`, `33`, etc.
3. Make `ConversationHandler` state returns consistent.
   - Ensure handlers always return a valid next state or `ConversationHandler.END`.

### Session 2

1. Rewrite `robot_queries.py` into a proper HTTP client layer.
   - Return parsed data instead of raw `ClientResponse`.
   - Add timeouts and explicit `404/409/5xx` handling.
2. Centralize runtime configuration.
   - `TOKEN`, `API_URL`, `ADMIN_USERNAME`, `DONATE_URL`, ports, and similar values.
3. Protect `/api/sendMessageForUser`.
   - Add authentication and typed request models.

### Session 3

1. Standardize on a single Telegram SDK.
   - Remove `telebot`/`pyTelegramBotAPI`/extra `telegram` usage and keep one library.
2. Normalize bot + server lifecycle.
   - Replace the current thread-based startup with a clearer orchestration model.
3. Fix `ConversationHandler` structure instead of suppressing warnings.

## Suggested Execution Order

1. Global notification state.
2. `robot_queries.py`.
3. One Telegram SDK.
4. HTTP endpoint authentication.
5. App lifecycle cleanup.
6. Config cleanup and follow-up refactors.
