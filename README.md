# Практическая работа 1.7.1 — калькулятор ASP.NET Core MVC в Docker

Проект соответствует заданию из пособия (с. 31–35, приложение 2, с. 109–111): сложение, вычитание, умножение и деление; Dockerfile; публикация исходников в GitHub; сборка, запуск и остановка контейнера. Номер студента: **13**. Внешний порт: **5013**, внутренний порт приложения: **5055**.

## Перед сдачей

1. Откройте `Views/Shared/_Layout.cshtml` и замените `УКАЖИТЕ СВОИ ФИО` и `УКАЖИТЕ СВОЮ ГРУППУ` на свои данные. Это обязательное условие для студентов УрФУ.
2. Установите .NET 8 SDK и Docker Desktop (Windows/macOS) либо Docker Engine (Linux); при работе в Visual Studio выберите workload «ASP.NET и разработка веб-приложений». Убедитесь, что Docker запущен.
3. Распакуйте архив и откройте `Calculator.csproj` в Visual Studio. Запустите профиль `Calculator` (кнопка ▶) или в терминале из папки с `Calculator.csproj` выполните `dotnet run`. Откройте `http://localhost:5055/`. Проверьте, например, `8 + 2 = 10`, `8 / 2 = 4`, деление на ноль.

## GitHub: куда выгрузить

В GitHub создайте **свой новый репозиторий**, например `calculator-docker-13`, без автоматического README (он уже есть). В терминале из папки `Calculator`, где лежат `Calculator.csproj` и `Dockerfile`:

```bash
git init
git add .
git commit -m "Add ASP.NET Core MVC calculator and Dockerfile"
git branch -M main
git remote add origin https://github.com/ВАШ_ЛОГИН/calculator-docker-13.git
git push -u origin main
```

Замените `ВАШ_ЛОГИН`. Если Git запросит учётные данные, пройдите вход через менеджер учётных данных или используйте личный токен вместо пароля. Если имя/email Git не настроены, задайте `git config --global user.name "Ваше имя"` и `git config --global user.email "ваша-почта"` перед commit. Убедитесь в браузере, что в репозитории видны исходники, Dockerfile и README. **Не клонируйте пример преподавателя вместо собственного репозитория:** в отчёте должен быть ваш GitHub.

## Локальная сборка Docker

В той же папке `Calculator`:

```bash
docker --version
docker build -t 13-calculator:latest .
docker images
docker run -d --name 13-calculator-container -p 5013:5055 13-calculator:latest
docker ps
```

Откройте `http://localhost:5013/`. Если имя контейнера занято после предыдущего запуска, используйте `docker start 13-calculator-container` либо удалите прежний контейнер командой `docker rm -f 13-calculator-container` и повторите `docker run`.

## Сервер кафедры

Данные доступа держите только у себя. Подключитесь в терминале (Windows PowerShell, macOS Terminal или Linux):

```bash
ssh student@93.88.178.186
```

Введите пароль из задания при запросе SSH (символы при вводе не показываются). При первом подключении сверьте отпечаток ключа сервера с данными кафедры, затем подтвердите его. После входа выполните:

```bash
pwd
ls -l
git clone https://github.com/ВАШ_ЛОГИН/calculator-docker-13.git
cd calculator-docker-13
sudo docker build -t 13-calculator:latest .
sudo docker images
sudo docker run -d --name 13-calculator-container -p 5013:5055 13-calculator:latest
sudo docker ps
```

Пароль `sudo` может запрашиваться отдельно. Откройте `http://93.88.178.186:5013/` в браузере на своём компьютере. Если порт 5013 занят, сначала проверьте `sudo docker ps --format 'table {{.Names}}\t{{.Ports}}'` и выясните, кому принадлежит порт; не останавливайте чужой контейнер. Если порт должен быть выделен именно вам, согласуйте конфликт с преподавателем. Если страница не открывается, проверьте `sudo docker logs 13-calculator-container`, `sudo docker ps -a` и доступность порта снаружи.

Для повторного обновления после изменения файлов в GitHub: `git pull`, `sudo docker rm -f 13-calculator-container`, затем снова `sudo docker build ...` и `sudo docker run ...` из команд выше.

## Демонстрация запуска и остановки

```bash
sudo docker ps -a
sudo docker stop 13-calculator-container
sudo docker ps -a
sudo docker start 13-calculator-container
sudo docker ps
```

После `stop` страница на порту 5013 перестанет отвечать; после `start` снова заработает. Оставьте контейнер в том состоянии, которое просит преподаватель. При необходимости удалить **свой** контейнер: `sudo docker rm -f 13-calculator-container`. Образ удаляйте только после удаления контейнера: `sudo docker rmi 13-calculator:latest`.

## Что включить в отчёт

По пособию (с. 35) добавьте описание шагов и **свои реальные** снимки экрана: (1) Visual Studio с `appsettings.json`; (2) свой репозиторий GitHub; (3) консоль после `git clone`; (4) список образов `docker images`; (5) запущенный контейнер `docker ps`; (6) страницу калькулятора на `http://93.88.178.186:5013/`. Приложите листинги `Controllers/CalculatorController.cs`, `appsettings.json`, `appsettings.Development.json`, `Program.cs`; дополнительно полезны `Dockerfile` и `Views/Calculator/Index.cshtml`. Зафиксируйте результат остановки и повторного запуска контейнера. Не помещайте пароль SSH или персональные токены в отчёт, репозиторий или снимки экрана.

## Структура

- `Controllers/CalculatorController.cs` — обработка GET/POST и вычисление.
- `Models/CalculatorInput.cs` — входные данные и проверка обязательных чисел.
- `Views/Calculator/Index.cshtml` — форма и результат.
- `Views/Shared/_Layout.cshtml` — оформление и данные студента в футере.
- `appsettings.json` — HTTP-прослушивание `0.0.0.0:5055` внутри контейнера.
- `Dockerfile` — сборка .NET 8 SDK и запуск через ASP.NET Core runtime.

Порт из пособия `5001` — иллюстрация для студента №1. Для №13 внешний порт `5013`; запись `-p 5013:5055` означает «порт сервера 5013 → порт контейнера 5055» и соответствует предоставленному примеру команды. В Dockerfile и приложении внутренний порт согласован.
