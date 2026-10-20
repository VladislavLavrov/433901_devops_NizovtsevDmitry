# ПР3 — Калькулятор с Apache Kafka и MariaDB

Студент №13: Низовцев Дмитрий Алексеевич, НМТ-433901. Основа: п. 1.7.3 пособия, с. 42–45 и приложение 4, с. 118–134. Новую версию размещаем в отдельной ветке **kafka** существующего репозитория.

## Что изменилось

ПР2: HTTP → контроллер вычисляет → MariaDB.

ПР3: HTTP → KafkaProducerService → топик запросов → KafkaConsumerService вычисляет → MariaDB → страница результата. Дополнительно консьюмер публикует сообщение в топик результатов, видимый в Kafka UI.

Контроллер только проверяет ввод и отправляет JSON. Расчёт и SaveChangesAsync выполняются в KafkaConsumerService, зарегистрированном как BackgroundService. Отдельная страница Result ожидает запись с GUID запроса в базе и обновляется каждые 2 секунды. Kafka UI позволяет независимо проверить сообщения, ключи, partition и offset. Результаты вычислений не исчезают после чтения сообщения консьюмером.

В отличие от примера приложения 4, консьюмер сохраняет результат через собственный DI scope и DbContext, без HTTP Callback к тому же приложению. Это сохраняет передачу задания через Kafka и убирает зависимость от внутреннего HTTP-адреса обратного вызова. Топик результатов служит для наблюдения за обработкой; страница результата читает уже сохранённую запись MariaDB.

Использованы .NET 8, EF Core 8.0.14, Pomelo 8.0.3, Confluent.Kafka **2.8.0**, Apache Kafka **3.9.1**, MariaDB **10.11**, Kafka UI **v0.7.2**. Kafka работает в KRaft (broker + controller в одном учебном узле), ZooKeeper не нужен.

## Адреса и имена

| Назначение | Значение |
|---|---|
| Калькулятор | http://93.88.178.186:5313/ |
| Kafka UI | http://93.88.178.186:10113/ |
| MariaDB для HeidiSQL | 93.88.178.186:5043 |
| База | 13-WebCalculatorDb |
| Пользователь базы | calculator13 |
| Внутренний Kafka | kafka:9092, без публикации этого listener в Интернет |
| Kafka для локальной отладки/SSH-туннеля | 127.0.0.1:19013 на сервере |
| Топик запросов | nizovtsev13-calculation-requests |
| Топик результатов | nizovtsev13-calculation-results |
| Топик некорректных сообщений | nizovtsev13-calculation-errors |
| Группа консьюмера | nizovtsev13-calculator-workers |

`myWebCalculatorDb` в листинге пособия — пример имени базы. В работе сохраняется индивидуальное имя `13-WebCalculatorDb`, применённое в ПР2 и соответствующее примерам с номером студента в тексте пособия. Веб-порт 5313 сохраняется, поскольку 5013 на общем сервере занят. Другие порты перед запуском проверяются.

**ВАЖНО:** Compose намеренно сохраняет имя проекта **nizovtsev13-pr2**, контейнеры mariadb/web и том mariadb-data из ПР2. Это обновление существующего стека, а не создание второй базы поверх занятых портов. Не меняйте `name:` и не используйте `docker compose down -v`.

## Этап 1 — подготовить код на своём компьютере

1. Откройте вашу локальную папку, из которой делаете коммиты. Корень — там, где Calculator.csproj, Dockerfile и docker-compose.yml.
2. Перед заменой файлов проверьте Git и создайте ветку:

```bash
git status
git pull --ff-only
git switch -c kafka
```

Выполняйте pull/switch при чистом рабочем дереве. Если у вас есть незакоммиченные изменения ПР2, сначала сохраните и закоммитьте их. Если ветка kafka уже существует, вместо создания выполните `git switch kafka` и проверьте её содержимое. Не используйте принудительное переключение.

3. Распакуйте ZIP в отдельную временную папку. В этом архиве Calculator.csproj уже лежит в корне, без дополнительной папки Calculator. Скопируйте **всё содержимое распакованного архива** в корень локального проекта с заменой совпадающих файлов. Не копируйте саму внешнюю папку распаковки: не должно получиться Calculator/Calculator.csproj рядом с корневым Calculator.csproj. Папку .git и существующий .env сохраняйте. Комплект основан на нашей версии ПР2; самостоятельные изменения, отсутствующие в диалоге, предварительно сохраните и перенесите отдельно.
4. Откройте Calculator.csproj в Visual Studio. Восстановите NuGet и выполните сборку. Для этого проекта нужен .NET 8 SDK. F5 без доступных Kafka и базы не проверяет весь конвейер; основной запуск далее выполняется через Compose.
5. В футере уже указаны ваши ФИО, группа и №13. Новые файлы — Services/KafkaProducerService.cs, KafkaConsumerService.cs, KafkaOptions.cs, KafkaJson.cs, Models/CalculationRequest.cs, CalculatorLibrary.cs, CalculationResultViewModel.cs, Views/Calculator/Result.cshtml и миграция AddKafkaRequestId.

## Этап 2 — выгрузить ветку kafka в GitHub

В той же локальной папке:

```bash
git branch --show-current
git status
git add .
git commit -m "Add Kafka producer consumer and result page"
git push -u origin kafka
```

Первая команда должна показать **kafka**. На GitHub переключите список веток с master на kafka и проверьте новые файлы и Compose. Не сливайте эту работу в master до требования преподавателя — ПР2 останется в основной ветке. .env с реальными паролями не добавляйте.

## Этап 3 — развернуть через PuTTY

### 3.1 Войти и получить ветку

PuTTY: Host Name 93.88.178.186, Port 22, SSH, Open. Логин student, пароль сервера — из исходного задания. Пароль не отображается при вводе. На сервере:

```bash
cd ~/433901_devops_NizovtsevDmitry
git status --short
git fetch origin
git switch --track origin/kafka
git branch --show-current
```

Если локальная ветка kafka на сервере уже есть:

```bash
git switch kafka
git pull --ff-only
```

Если Git не даёт переключиться из-за локальных изменений — пришлите вывод git status; не выполняйте reset --hard или git clean.

Проверьте единственный экземпляр проекта:

```bash
find . -name Calculator.csproj -not -path '*/obj/*' -not -path '*/bin/*'
```

Ожидается `./Calculator.csproj`. Это предотвращает повторение ошибки с вложенной копией проекта из ПР2.

### 3.2 Сохранить настройки ПР2

На сервере уже есть .env. **Не копируйте поверх него .env.example и не меняйте существующие пароли MariaDB.** Откройте:

```bash
nano .env
```

Оставьте WEB_PORT=5313, DB_PORT=5043 и первоначальные DB_ROOT_PASSWORD/DB_PASSWORD. Добавьте, если строк ещё нет:

```text
KAFKA_UI_PORT=10113
KAFKA_LOCAL_PORT=19013
```

В nano: Ctrl+O, Enter, Ctrl+X. Для проверки новых портов:

```bash
sudo ss -ltn '( sport = :10113 or sport = :19013 )'
sudo docker compose version
sudo docker compose ps
```

Если порт занят вашим уже созданным Kafka/UI, повторный Compose обновит его. Если порт занят чужим сервисом, согласуйте свободный порт и измените соответствующее значение .env, не трогая чужой контейнер. Порт 10025 из пособия относится к показанному в нём UI, а не к обязательному общему сервису; в нашем стеке свой UI на 10113.

Для первого запуска без прежней ПР2: создайте .env из .env.example ОДИН раз и задайте собственные учебные пароли. Нельзя оставлять заглушки KEEP_EXISTING... . Для существующей базы нужны её прежние пароли.

### 3.3 Собрать, затем запустить

```bash
sudo docker compose build web
sudo docker compose up -d --build
sudo docker compose ps -a
sudo docker compose logs --tail=80 kafka
sudo docker compose logs --tail=100 web
```

Сначала завершается сборка, затем Compose запускает/проверяет MariaDB и Kafka, однократно создаёт топики и обновляет web. Не нужно вручную останавливать контейнер web из ПР2: имя проекта/сервиса сохранено и Compose заменяет его сам. Базу и её том не удаляйте.

Ожидаемые сервисы:

| Сервис | Ожидаемое состояние |
|---|---|
| mariadb | Up (healthy) |
| kafka | Up (healthy) |
| kafka-init | Exited (0), это нормальное завершение создания топиков |
| kafka-ui | Up |
| web | Up |

Kafka при первом старте может готовиться несколько минут. Консьюмер может получить partition после небольшой задержки. В web ожидаются сообщения об миграции AddKafkaRequestId, запуске HTTP на 5055 и Consumer started. Если web перезапускается, читайте журнал, не считайте контейнер работоспособным только по факту создания.

Проверка топиков без браузера:

```bash
sudo docker compose exec kafka /opt/kafka/bin/kafka-topics.sh --bootstrap-server kafka:9092 --list
```

Новые контейнеры: nizovtsev13-kafka, nizovtsev13-kafka-ui, nizovtsev13-kafka-init. Образ web: 13-calculator-kafka:latest. Пользуйтесь `sudo docker images` для скриншота собранных/скачанных образов.

## Этап 4 — HeidiSQL

Если HeidiSQL уже установлен после ПР2, переустанавливать не нужно. Используйте существующий сеанс:

| Поле | Значение |
|---|---|
| Тип | MariaDB or MySQL (TCP/IP) |
| Хост | 93.88.178.186 |
| Порт | 5043 / фактический DB_PORT |
| Пользователь | calculator13 |
| Пароль | первоначальный DB_PASSWORD из .env |
| База | 13-WebCalculatorDb |

Пароль SSH student к MariaDB не относится. Если в ПР2 вы использовали предложенный CalcStudy13, он остаётся тем же — если выбрали другой, используйте его. Скачивание для первого подключения: https://www.heidisql.com/download.php . Если внешнее подключение закрыто, в PuTTY настройте локальный туннель 15043 → 127.0.0.1:5043 и в HeidiSQL укажите 127.0.0.1:15043.

## Этап 5 — миграции и сохранность старых записей

Миграция m_init из ПР2 сохранена с прежним ID. Новая миграция **20261020000100_AddKafkaRequestId** добавляет nullable-столбец RequestId varchar(36) и уникальный индекс. Старые строки останутся, у них RequestId=NULL. Новые строки содержат GUID, совпадающий с ключом Kafka. Уникальный индекс предотвращает повторную DB-запись для одного запроса при повторном чтении сообщения.

Обе миграции уже в архиве. При `Database__MigrateOnStartup=true` новая применяется автоматически, до запуска hosted consumer. **Повторно Add-Migration m_init/AddKafkaRequestId не выполняйте.** В HeidiSQL:

```sql
SELECT * FROM `13-WebCalculatorDb`.`__EFMigrationsHistory`;
SHOW COLUMNS FROM `13-WebCalculatorDb`.`DataInputVariants`;
```

Ожидаются две миграции и поле RequestId. Таблицу вручную не создавайте.

Если преподавателю нужна демонстрация Update-Database в Visual Studio: откройте консоль диспетчера пакетов, Default project Calculator, задайте строку подключения переменной окружения и выполните Get-Migration / Update-Database. Пароль берётся из .env сервера:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=93.88.178.186;Port=5043;Database=13-WebCalculatorDb;User=calculator13;Password=ВАШ_ПАРОЛЬ_БАЗЫ;'
Get-Migration
Update-Database
```

Строку с паролем на снимки не включайте. Генерация новых миграций нужна только при следующем изменении модели; уже применённые миграции не удаляйте и не переименовывайте.

## Этап 6 — калькулятор, топики и база

### 6.1 Сделать расчёт

1. Откройте http://93.88.178.186:5313/ , Ctrl+F5.
2. Введите 8, выберите +, введите 2, нажмите **«Отправить в Kafka»**.
3. Откроется страница «Результат через Kafka» с GUID. Если запись ещё не обработана, страница обновляется каждые 2 секунды.
4. Ожидается результат 10 и номер записи MariaDB. Сделайте ещё 8−2=6, 8×2=16, 8÷2=4. Деление на ноль отклоняется формой и не отправляется в топик.

### 6.2 Посмотреть сообщения Kafka

1. Откройте http://93.88.178.186:10113/ . Если меняли KAFKA_UI_PORT, подставьте свой порт.
2. Выберите кластер **nizovtsev13**, раздел **Topics**.
3. Откройте **nizovtsev13-calculation-requests** → **Messages**, выберите чтение с начала (Earliest/Oldest/Beginning, название зависит от интерфейса), запустите просмотр сообщений. При необходимости выберите формат String/JSON.
4. Найдите JSON с числами 8 и 2, operation Add, requestId, sentAtUtc. Ключ сообщения — тот же GUID, что на странице результата.
5. Откройте **nizovtsev13-calculation-results** → Messages. Найдите сообщение с тем же GUID, result и databaseId.
6. В **Consumer Groups** найдите nizovtsev13-calculator-workers. После обработки lag должен вернуться к 0. Это дополнительно подтверждает чтение запросов консьюмером.

UI настроен на чтение: для проверки сообщений ничего вручную публиковать в нём не нужно. Третий топик errors используется только для некорректных внешних сообщений и при обычной работе может быть пустым.

Если UI не открывается снаружи при работающем контейнере, добавьте PuTTY-туннель Source port 10113 → Destination 127.0.0.1:10113 (Local, Add, Apply) и откройте http://127.0.0.1:10113/ на своём компьютере. Если внешний KAFKA_UI_PORT другой, измените Destination, Source можно оставить 10113.

### 6.3 Проверить MariaDB

В HeidiSQL откройте DataInputVariants, обновите вкладку «Данные». Или выполните scripts/verify_database.sql. Для конкретного GUID:

```sql
SELECT ID_DataInputVariant, RequestId, Operand_1, Operand_2, Type_operation, Result
FROM `13-WebCalculatorDb`.`DataInputVariants`
ORDER BY ID_DataInputVariant DESC LIMIT 30;
```

У новых строк RequestId не пустой; он совпадает с ключом в обоих топиках. Это связывает веб-запрос, Kafka и запись БД. Сохранены пять прежних полей из ПР2; исходные числа и результат по-прежнему varchar(128).

### 6.4 Автоматическая проверка четырёх операций

В PuTTY из корня проекта:

```bash
python3 scripts/smoke_test.py http://127.0.0.1:5313
```

Скрипт проходит AntiForgery-форму, отправляет четыре реальных запроса, получает GUID каждого и ждёт сохранённый результат через HTTP Status. Ожидаются четыре PASS с числами 10, 6, 16, 4. Скрипт добавляет четыре строки в историю. Он не подменяет консьюмер, Kafka или базу; для работы нужен полностью запущенный стек.

## Как обеспечивается повторная обработка

- Producer настроен на EnableIdempotence и Acks=All в пределах одного брокера.
- Consumer отключает автокоммит и фиксирует offset только после сохранения в MariaDB и подтверждения публикации результата.
- При временной ошибке обрабатывается повторно то же сообщение, а не следующее.
- GUID RequestId и уникальный индекс защищают БД от дублей одного запроса при повторном чтении.
- Между MariaDB и Kafka нет общей транзакции: сообщение результата может появиться повторно после сбоя между отправкой и commit. Это учебная at-least-once обработка, а не гарантия exactly-once для всей системы. Повторная отправка формы пользователем создаёт новый GUID и считается новым запросом.
- Один брокер и replication factor 1 достаточны для практической работы, но не обеспечивают отказоустойчивый кластер.

## Локальный запуск и отладка

Для полного локального запуска нужны Docker Desktop/Engine и .env с локальными учебными паролями. Из корня проекта: `docker compose up -d --build`, затем http://localhost:5313/ и http://localhost:10113/ . Это создаёт отдельные локальные тома.

Для F5/`dotnet run` вне контейнера должны быть доступны БД и Kafka. appsettings.Development.json предполагает Kafka на 127.0.0.1:19013. На сервере этот listener опубликован только на loopback. В PuTTY можно пробросить Local 19013 → 127.0.0.1:19013; для БД использовать прямой 5043 или отдельный туннель. Строку подключения передайте через переменную окружения/Manage User Secrets. Не запускайте дополнительный локальный consumer в той же группе одновременно с серверным, если хотите однозначно показать обработку на сервере.

## Остановка и последующие обновления

Обновить ветку на сервере:

```bash
git pull --ff-only
sudo docker compose up -d --build
```

Остановить весь свой стек: `sudo docker compose stop`. Вернуть с проверкой зависимостей: `sudo docker compose up -d`. Не запускайте одновременно ПР2 и ПР3 в разных папках с одинаковым именем проекта как независимые приложения — они управляют теми же сервисами.

**Не используйте `down -v`**: это удалит историю MariaDB и Kafka. Не меняйте CLUSTER_ID у уже созданного Kafka-тома. Kafka по умолчанию хранит сообщения 168 часов; это не влияет на уже сохранённые строки MariaDB.

Если нужно вернуться к ПР2, сначала сохраните текущие изменения в Git, переключитесь на ветку ПР2 и выполните compose up -d --build web. Новая nullable-колонка не мешает старой версии. Kafka/UI можно остановить до переключения ветки командами `sudo docker compose stop kafka-ui kafka`. Не удаляйте базу.

## Типовые ошибки

| Симптом | Что проверить |
|---|---|
| Сборка: duplicate class / top-level statements | Вложенная копия проекта. find должен показывать один csproj |
| Kafka unhealthy | docker compose logs --tail=100 kafka; дождаться первого запуска; проверить память и права тома |
| kafka-init Exited (1) | docker compose logs kafka-init; Kafka должна быть healthy |
| kafka-init Exited (0) | Норма: топики созданы, одноразовый сервис завершился |
| Страница ждёт результат | logs web; consumer group; правильный topic и BootstrapServers |
| Топик пуст в UI | Выбрать начало/earliest, обновить Messages, сделать новый расчёт |
| История есть, результат-топик запаздывает | DB записывается перед публикацией результата; обновить UI и проверить producer logs |
| Access denied MariaDB | Нужен исходный пароль ПР2; изменение .env не меняет пароль существующего пользователя |
| Порт занят | Проверить docker ps/ss; не удалять чужие контейнеры |

Команды диагностики (не публикуйте значения .env):

```bash
sudo docker compose ps -a
sudo docker compose logs --tail=100 web
sudo docker compose logs --tail=100 kafka
sudo docker compose logs kafka-init
```

## Что заскринить для отчёта по п. 1.7.3

Обязательные рисунки из пособия (с. 45):
1. appsettings.json в Visual Studio.
2. appsettings.Development.json в Visual Studio.
3. Клонирование/получение ветки на сервере (git fetch/switch/pull; указать, что репозиторий использован после ПР2).
4. Созданные образы (`sudo docker images`).
5. HeidiSQL с заполненной DataInputVariants, включая RequestId новых строк.
6. Kafka UI → Topics → Messages с раскрытым JSON запроса; дополнительно результат с тем же GUID.
7. Браузер со страницей результата, GUID, вашими ФИО и группой, видимым адресом 5313.

Полезно также: GitHub с выбранной веткой kafka и открытым Compose; `docker compose ps -a` с сервисами; журнал Produced/Processed/Committed; две миграции в HeidiSQL; четыре PASS smoke-теста.

В приложения отчёта нужны листинги контроллера, **KafkaConsumerService.cs**, appsettings.json, appsettings.Development.json, Program.cs. Дополнительно можно включить producer и Compose. Пароли .env и SSH не вставляйте в репозиторий/отчёт.

## Кратко для защиты

«Создал ветку kafka. Контроллер отправляет JSON с GUID через producer в топик запросов. BackgroundService consumer получает сообщение, вычисляет результат и сохраняет его в MariaDB, затем публикует результат во второй топик и фиксирует offset. Страница результата находит запись по GUID. Через Kafka UI показываю сообщения, а через HeidiSQL — те же расчёты и GUID в базе. Compose управляет MariaDB, Kafka, Kafka UI и приложением; kafka-init однократно создаёт топики».

## Ограничения проверки подготовленного комплекта

В среде подготовки нет .NET SDK и Docker. Компиляция, Docker build, применение миграции и реальный Kafka-конвейер здесь не выполнялись. JSON, XML, YAML и связи Compose проверены статически; smoke-тест предназначен для реальной проверки после запуска на вашем сервере. Не включайте результаты тестов в отчёт до фактического выполнения.

Официальные материалы:
- https://kafka.apache.org/39/getting-started/quickstart/
- https://github.com/apache/kafka/blob/trunk/docker/examples/docker-compose-files/single-node/plaintext/docker-compose.yml
- https://docs.confluent.io/kafka-clients/dotnet/current/overview.html
- https://www.nuget.org/packages/Confluent.Kafka/2.8.0
- https://github.com/provectus/kafka-ui
