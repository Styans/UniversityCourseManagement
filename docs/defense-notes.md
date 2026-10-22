# Шпаргалка к защите — задание 2 (модули 5–8)

## Перед защитой (5 минут)

1. `powershell -ExecutionPolicy Bypass -File scripts/setup-dev-secrets.ps1` — ключ JWT в User Secrets (если ещё не делали).
2. Запустить `UniversityApi` (профиль `http`) → http://localhost:5096/swagger.
3. Запустить `UniversityClient` (профиль `http`) → http://localhost:5210.
4. В Postman импортировать обе коллекции из папки `postman/` и один раз прогнать «задание 2» целиком (Run collection) — всё зелёное.
5. Демо-пароль у всех пользователей: `Demo#2026` (`admin`, `teacher.abenov`, `teacher.sadykova`, `student.bekova`, `student.omarov`).

---

## 1. Аутентификация и авторизация — в чём разница

| | Аутентификация | Авторизация |
|---|---|---|
| Вопрос | **Кто ты?** | **Что тебе можно?** |
| В проекте | `POST /api/auth/login` проверяет пароль → выдаёт JWT; дальше `UseAuthentication()` проверяет JWT в каждом запросе | `UseAuthorization()` проверяет `[Authorize]`, роли, политики; сервис проверяет, твой ли ресурс |
| Ошибка | **401 Unauthorized** — нет токена / токен плохой | **403 Forbidden** — токен хороший, но прав нет |
| Пример | Без токена `DELETE /api/v1/courses/1` → 401 | `student.bekova` с верным токеном `DELETE /api/v1/courses/1` → 403 |

Фраза для ответа: *«Аутентификация устанавливает личность пользователя, авторизация решает, что этой личности разрешено. 401 — система не знает, кто ты; 403 — знает, но не пускает».*

## 2. Как связаны Identity, JWT, middleware, Claims и Roles

```
POST /api/auth/login {userName, password}
  └─ AuthService
       ├─ UserManager.FindByNameAsync        ← Identity: учётная запись из AspNetUsers
       ├─ SignInManager.CheckPasswordSignInAsync(lockoutOnFailure: true)
       │      ← Identity сравнивает хеш пароля (PBKDF2), считает неудачные попытки
       ├─ UserManager.GetRolesAsync           ← роли из AspNetUserRoles
       └─ JwtTokenService.CreateAccessToken   → JWT, подписанный ключом из User Secrets
            claims: sub, name, role…, student_id / teacher_id, iss, aud, exp

GET /api/enrollments   Authorization: Bearer eyJ...
  ├─ UseRouting()          — найден endpoint EnrollmentsController.GetAll
  ├─ UseCors()             — для запросов из браузера
  ├─ UseAuthentication()   — JwtBearer проверяет подпись, iss, aud, exp
  │                          → HttpContext.User = ClaimsPrincipal(claims из токена)
  ├─ UseAuthorization()    — [Authorize] / [Authorize(Policy = …)] по claims "role"
  │                          нет пользователя → 401 (OnChallenge); нет роли → 403 (OnForbidden)
  └─ Controller → EnrollmentService
         └─ IAuthorizationService.AuthorizeAsync(User, ресурс, операция)
               → EnrollmentAuthorizationHandler: student_id/teacher_id из токена == владелец?
               → нет → ReturnResult 403
```

- **Identity** — хранит пользователей, хеши паролей, роли; проверяет пароль; блокирует после 5 ошибок. Используется только при входе/регистрации/управлении ролями.
- **JWT** — «пропуск», который Identity-пользователь получает после входа. Сервер не хранит сессию: всё нужное (id, роли) внутри токена, а подпись гарантирует, что токен не подделан.
- **Authentication Middleware** — превращает JWT из заголовка в `HttpContext.User` (`ClaimsPrincipal`).
- **Claims** — утверждения о пользователе внутри токена: `sub`, `name`, `role`, `student_id`, `teacher_id`.
- **Roles** — частный случай claims (`role`). `[Authorize(Roles/Policy)]` и `User.IsInRole()` смотрят на них.
- **Authorization Middleware** — сравнивает claims с требованиями endpoint (`[Authorize]`, политики).

**Порядок middleware важен:** `UseRouting → UseCors → UseAuthentication → UseAuthorization → MapControllers`. Если поставить `UseAuthorization` раньше `UseAuthentication`, пользователь всегда будет анонимным и всё защищённое даст 401.

## 3. Почему проверки роли недостаточно

Роль отвечает на вопрос «может ли **преподаватель** менять оценки» — да. Но она не говорит, **чьи**. Если проверять только `[Authorize(Roles = "Teacher")]`, Абенов сможет поставить 10 баллов студенту в курсе Садыковой.

Поэтому есть второй уровень — **проверка по ресурсу** (resource-based authorization):
- `EnrollmentAuthorizationHandler`: Teacher может менять оценку, только если `course.TeacherId == teacher_id` из его токена → иначе 403 `NOT_COURSE_TEACHER` (сценарий 8).
- `StudentAuthorizationHandler`, `EnrollmentService.GetAllAsync`: Student видит только записи с `StudentId == student_id` → запрос чужих оценок → 403 (сценарий 9).
- Проверка на сервере, а не в клиенте: скрытая кнопка в интерфейсе не защищает — запрос можно отправить из Postman.

Ещё пример: Student запрашивает `GET /api/students/99999` (несуществующий) → 403, а не 404 — проверка прав идёт до поиска, иначе по 404/403 можно было бы перебором узнать, какие id существуют.

## 4. JWT — частые вопросы

- **Что внутри?** Три части `header.payload.signature` в Base64Url. Payload любой может прочитать (jwt.io), поэтому там нет паролей и персональных данных — только id, имя, роли.
- **Почему нельзя подделать?** Подпись HMAC-SHA256 вычисляется секретным ключом. Изменили `role` на `Admin` → подпись не сходится → 401 `INVALID_TOKEN` (в Postman — запрос «изменённый JWT», сценарий 5).
- **Что проверяется?** Подпись, `iss` (издатель), `aud` (аудитория), `exp` (срок), алгоритм только HS256 (токен с `alg: none` отклоняется). `ClockSkew = 0` — без стандартного «запаса» 5 минут.
- **Где ключ?** В User Secrets (`%APPDATA%\Microsoft\UserSecrets\…\secrets.json`) — вне проекта и Git. В Production — переменная окружения `Jwt__Key`; без неё приложение не стартует. В `appsettings.json` поле `Key` пустое.
- **Срок жизни?** 15 минут. Короткий срок ограничивает ущерб, если токен украдут.
- **Как отозвать токен до истечения?** JWT без состояния, поэтому — никак, только ждать `exp`. Если нужно мгновенно: refresh-токены + короткий access, чёрный список `jti` или проверка `SecurityStamp` в `OnTokenValidated` (Identity уже меняет `SecurityStamp` при смене ролей). Это возможное развитие.
- **Админ назначил роль — почему сразу не работает?** Роли записаны в уже выданный токен. Пользователь должен войти заново.
- **Как показать просроченный токен?** `POST /api/diagnostics/expired-token` (только Development) выдаёт копию вашего токена с `exp` в прошлом — подпись настоящая, но срок истёк → 401 `TOKEN_EXPIRED`.

## 5. Identity — частые вопросы

- **Как хранится пароль?** `PasswordHasher` Identity: PBKDF2 (HMAC-SHA512, соль, много итераций) → `AspNetUsers.PasswordHash`. Сам пароль нигде не хранится и не логируется.
- **Требования к паролю:** ≥ 8 символов, цифра, строчная, заглавная (`SecurityExtensions.AddAppIdentity`).
- **Блокировка:** 5 неверных паролей → 5 минут блокировки (`lockoutOnFailure: true`). Admin может снять: `POST /api/users/{id}/unlock`.
- **Одинаковые имена:** проверка в `AuthService.RegisterAsync` (409) + уникальный индекс `UserNameIndex` в БД.
- **Почему «неверный логин» и «неверный пароль» дают один ответ?** Чтобы нельзя было узнать, существует ли пользователь.
- **Регистрация не даёт админа:** в `RegisterDto` нет поля роли; всегда добавляется `Student`. Даже если прислать `"role": "Admin"` — поле игнорируется.
- **Самоповышение привилегий:** роли меняются только через `/api/users/{id}/roles` с политикой `AdminOnly` → Student получает 403 (сценарий 11). Admin не может снять роль Admin сам с себя (409).
- **Связь со Student/Teacher:** `ApplicationUser.StudentId` / `TeacherId` (FK, уникальный индекс, `OnDelete SetNull`), меняет Admin: `PUT /api/users/{id}/profile`.
- **`AddIdentityCore`, а не `AddIdentity`:** API работает только с JWT, cookie-аутентификация Identity не нужна.

## 6. Версии API

- Пакет `Asp.Versioning`, версия в сегменте URL: `/api/v1/courses`, `/api/v2/courses`.
- Два контроллера `Controllers/V1/CoursesController` и `Controllers/V2/CoursesController`, общий сервис `ICourseService` — фильтрация реализована один раз (`LoadFilteredAsync`), отличается только DTO: `CourseDto` (v1, не менялся) и `CourseV2Dto` (преподаватель-объект + `enrolledStudentsCount`).
- Почему отдельный DTO: если добавить поля в `CourseDto`, изменится контракт v1 — клиенты, которые строго разбирают ответ, могут сломаться.
- Старый адрес `/api/courses` = v1 (`AssumeDefaultVersionWhenUnspecified`) — коллекция задания 1 работает без изменений адресов.
- Swagger: два документа (выпадающий список вверху справа), токен из Authorize сохраняется при переключении.
- `/api/v3/courses` → 404 `UNSUPPORTED_API_VERSION`; в ответах заголовок `api-supported-versions: 1.0, 2.0`.

## 7. CORS и клиент

- **CORS** — правило браузера: JavaScript со страницы одного origin (схема+хост+порт) не может прочитать ответ с другого origin, если сервер не разрешил это заголовком `Access-Control-Allow-Origin`.
- Политика `UniversityClients`: `http://localhost:5210` (наш MVC-клиент) и `http://localhost:3000` (SPA). Только GET/POST/PUT/DELETE и заголовки Authorization/Content-Type/Accept.
- **MVC-клиент делает запросы со своего сервера** — CORS их не касается. Для демонстрации — страница **CORS-демо**: на `http://localhost:5210/Home/Cors` запрос проходит, на `http://127.0.0.1:5210/Home/Cors` (другой origin) браузер блокирует.
- **IHttpClientFactory** — переиспользует соединения (нет исчерпания сокетов, как с `new HttpClient()` на каждый запрос) и учитывает смену DNS. Клиент: `AddHttpClient("UniversityApi", …)` + `IHttpClientFactory.CreateClient`.
- **Ошибки в клиенте:** API выключен → «API недоступен» (`HttpRequestException`); тайм-аут 10 с (`TaskCanceledException`); 404 → «Не найдено»; другие коды → сообщение из `ReturnResult`. Чтобы показать — остановить API и обновить страницу.
- Адрес API — `UniversityClient/appsettings.json → UniversityApi:BaseUrl`, в коде не захардкожен.

## 8. Демонстрация сценариев

**Swagger:** Authorize → вставить `accessToken` без слова Bearer. Чтобы сменить пользователя — Logout → Authorize заново.

| № | Где | Шаги | Ожидание |
|---|---|---|---|
| 1 | Swagger / клиент | без токена `GET /api/v1/courses` | 200 |
| 2 | Swagger | без токена `POST /api/v1/courses` | 401 `UNAUTHORIZED` |
| 3 | Swagger | `POST /api/auth/login` `student.bekova` / `WrongPassword1` | 401 `INVALID_CREDENTIALS`, `result: null` |
| 4 | Swagger | login `student.bekova` / `Demo#2026` → Authorize → `GET /api/auth/me` | 200 + токен; можно вставить токен в jwt.io и показать claims |
| 5 | Swagger | `POST /api/diagnostics/expired-token` → Logout → Authorize с этим токеном → `GET /api/auth/me` | 401 `TOKEN_EXPIRED` |
| 6 | Swagger | под `student.bekova` `DELETE /api/v1/courses/1` | 403 (см. примечание ниже) |
| 7 | Swagger | под `teacher.abenov` `GET /api/enrollments` → взять id → `PUT /api/enrollments/{id}/grade` `{ "grade": 95 }` | 200 |
| 8 | Swagger | под `teacher.abenov` `PUT /api/enrollments/2/grade` (курс Databases) | 403 `NOT_COURSE_TEACHER` |
| 9 | Swagger | под `student.bekova` `GET /api/enrollments?studentId=2` | 403 `FORBIDDEN` |
| 10 | Swagger | под `admin` `GET /api/users` → id нужного → `POST /api/users/{id}/roles` `{ "role": "Teacher" }` | 200, роль в `roles` |
| 11 | Swagger | под `student.bekova` `GET /api/auth/me` → свой id → `POST /api/users/{id}/roles` `{ "role": "Admin" }` | 403 |
| 12 | Клиент / Swagger | в клиенте переключатель «API v1 / API v2»; в Swagger документы v1 и v2 | обе версии 200, в v2 есть кафедра и число студентов |

В свежей базе: Bekova — studentId 1, Omarov — 2; запись 1 — Bekova в «Web Services» (курс Абенова), запись 2 — Bekova в «Databases» (курс Садыковой). Если база старая, id лучше брать из ответов (`/api/auth/me`, `/api/enrollments`), как это делает коллекция Postman.

**Сценарий 6 — 401 или 403?** В таблице задания написано 401, но в требовании 2.5: «403 Forbidden, когда пользователь аутентифицирован, но не обладает необходимыми правами». Студент с верным токеном аутентифицирован → 403. Без токена тот же запрос → 401. Если преподаватель спросит — показать оба варианта (в коллекции Postman они идут подряд).

## 9. Где это в коде

| Что | Файл |
|---|---|
| Порядок middleware | `UniversityApi/Program.cs` |
| Identity, JWT, политики, CORS, версии | `UniversityApi/Extensions/SecurityExtensions.cs` |
| Роли, политики, имена claims | `UniversityApi/Auth/AppRoles.cs` |
| Выдача JWT | `UniversityApi/Auth/JwtTokenService.cs` |
| Проверка владения ресурсом | `UniversityApi/Auth/ResourceAuthorization.cs`, `Services/Implementations/EnrollmentService.cs`, `StudentService.cs` |
| Регистрация, вход, `/me` | `Services/Implementations/AuthService.cs`, `Controllers/AuthController.cs` |
| Роли пользователей | `Services/Implementations/UserService.cs`, `Controllers/UsersController.cs` |
| Учётная запись + связь со Student/Teacher | `Models/ApplicationUser.cs`, `Data/Configurations/ApplicationUserConfiguration.cs` |
| Роли и демо-пользователи | `Data/Seed/IdentitySeeder.cs` |
| Версии курсов | `Controllers/V1/CoursesController.cs`, `Controllers/V2/CoursesController.cs`, `DTO/Courses/V2/CourseV2Dto.cs` |
| Swagger + JWT | `Extensions/SwaggerExtensions.cs` |
| 401/403 в формате ReturnResult | `SecurityExtensions.WriteUnauthorizedAsync / WriteForbiddenAsync`, `WebApplicationExtensions.UseUnifiedStatusCodePages` |
| Клиент + IHttpClientFactory | `UniversityClient/Program.cs`, `UniversityClient/Services/UniversityApiClient.cs` |
