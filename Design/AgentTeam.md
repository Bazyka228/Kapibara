# Команда разработки Second Project

Настроено 30 октября 2026. Изучен фактический Unity-проект и доступные инструменты. Настройка не меняет игровой код, графику, экономику или сохранения.

## Архитектура и источник правды

| Область | Реализация / путь |
|---|---|
| Рабочий проект | `C:/Users/fedor/Second Project` |
| Пространство разработки | `C:/Users/fedor/OneDrive/Документы/ChatGPT/Second Project` |
| Зеркало | `CurrentProject/`; не полный Unity-проект |
| Движок | Unity `6000.6.5f1`, 2D, uGUI `2.6.0`; браузерная доставка через WebGL |
| Сцена | `Assets/Capybara67/Scenes/Capybara67_Idle.unity` |
| UI | Программный partial-класс `CapyIdleGame`, CanvasScaler/BoardFit, reference canvas 480×850 |
| Модель | `IdleModel`, `IdleActivities`, `IdleMandarins`, `IdleSurprises`, `IdleTapProgress` |
| Каталоги | `IdleCatalog`, `CapyCompleteSets`; стабильные ID связаны с ownership/equipment |
| Отрисовка | `SoftShape`, `CapyContour`, текстуры Resources, `CapyUnifiedGarden` |
| Анимация | DOTween, `CapyTweenDriver`, motion-компоненты и пул VFX |
| SDK | Установленный PluginYG2 `v2.0092`, YandexGames `v1.0091`, RewardedAdv `v1.011` — версии прочитаны из файлов плагина |
| Сохранение | JSON `IdleSave`, primary `capy67.idle.v1`, `.backup`, legacy import, `Repair()` |

`UnityContent`, `LegacyArchive`, старый корневой README и старые разделы CURRENT_STATE содержат прежнюю архитектуру. Они не основание для замены текущего кода. HTML5 в задаче не означает миграцию на Phaser. Изменение движка требует отдельного запроса.

## Восемь ролей

| Роль | Основная ответственность | Граница |
|---|---|---|
| game-designer | Цикл, спецификация, детская понятность, acceptance criteria | Не правит runtime без назначения |
| gameplay-programmer | C#-логика, ввод, занятия, миграции | Не меняет общие файлы одновременно с экономистом |
| ui-ux-designer | HUD, меню, короткие подсказки, мобильная компоновка | Не меняет цены/сохранения ради верстки |
| game-artist | Декор, иконки, еда, окружение, импорт атласов | Капибары и наряды защищены |
| animation-specialist | DOTween, transitions, VFX, pause/calm/cleanup | Не начисляет игровые награды из визуального callback |
| qa-game-tester | Регрессии, миграции, жесты, кадры и отчёты | Не ослабляет тест ради PASS; профиль игрока не трогает |
| economy-balancer | Источники/траты, цены, вероятность и симуляции | Без необоснованных обещаний метрик и сбросов владения |
| yandex-games-specialist | PluginYG2, lifecycle, WebGL, реклама, платежные контракты | Не публикует/не включает платежи без задачи |

Главный агент — интегратор. В `.codex/config.toml` разрешены три одновременных дочерних агента; роли запускаются по необходимости, а не постоянно. Модель и права наследуются. Общие файлы передаются одному владельцу. Все агенты читают корневой AGENTS.md. Конфигурации — самостоятельные TOML с `name`, `description`, `developer_instructions`, согласно [официальному формату Codex](https://learn.chatgpt.com/docs/agent-configuration/subagents).

### Протокол поручения

1. Цель и критерий результата.
2. Разрешённые файлы, read-only зависимости и запрещённые области.
3. Входные данные/контракт с соседней ролью.
4. Проверки и формат отчёта.
5. Интегратор собирает изменения, проверяет исходники и запускает общий прогон один раз после завершения зависимых правок.

Пример: дизайнер готовит спецификацию нового занятия; художник создаёт отдельные декоративные ресурсы; программист реализует логику после согласования контракта; аниматор работает в отдельном компоненте; QA проверяет объединённый результат. Дизайнер и экономист могут анализировать параллельно, но не редактировать IdleModel одновременно.

## Защита графики и данных

Не изменять Characters, Outfits, CompleteSets, Accessories, fur masks и внешний вид капибары/нарядов без отдельного запроса. Это относится и к `CapyIdleLife`, `CapyIdlePortraits`, `CapyBakedOutfits`, `CapyPortraitMotion`, avatar helpers и eye shader: формально неизменный PNG не защищает от изменений общего материала. Сохранять `protectedArtDepth`, `.meta`, GUID, контрольные кадры и хеши.

Загрузчик сейчас принимает version 1. Простое увеличение save version без обновления чтения может скрыть старые данные. `Repair()` очищает неизвестные ID, поэтому удаление/переименование предмета требует миграции. Проверять старый JSON, backup, повторную загрузку/миграцию, покупки, оборудование и повторный reward callback. Запрещены сбросы реального PlayerPrefs и тестовые балансы в пользовательском профиле.

## Команды проверок

Рабочая директория команд — пространство разработки. Использовать уже установленный PowerShell 7; скрипты с Add-Type запускать в свежем процессе. Check-Activities включает Check-Model, повторять оба подряд не требуется.

```powershell
& 'C:/Users/fedor/.cache/codex-runtimes/codex-primary-runtime/dependencies/native/powershell/pwsh.exe' -NoProfile -File 'CurrentProject/ArtSource/CompleteSets/Check-Activities.ps1'
& 'CurrentProject/ArtSource/CompleteSets/Compile-Staging.ps1'
```

Первый проверяет настоящую модель с Unity/JSON-заглушками, не PlayerPrefs. Второй компилирует две сборки с Unity references из actual .csproj; список staged sources явный, новый C#-файл надо включить в него. Это не доказывает успешный импорт ресурсов/рендеринг. `Verification/Idle/Idle.csproj` и `Verification/Expansion/Expansion.csproj` ссылаются на старый код — не использовать для текущего baseline.

### Unity: только один владелец прогона

1. Проверить отсутствие RUNNING и незавершённых маркеров. Сверить изменённые live/staged файлы, сделать резервную копию, перенести согласованный набор с .meta.
2. Дождаться окончания компиляции/импорта. Выбрать разрешение Game View и оставить Game в фокусе для сценариев удержания.
3. В actual project создать `Temp/Capy67-garden-capture` для кадров, затем `Temp/Capy67-idle-checks` для полного прогона. Не создавать `Capy67-care-only`, если требуется полный набор.
4. Ничего не переносить и не редактировать в Unity Assets до завершения. Harness ставит SessionState validation, использует `.validation`, блокирует reload и восстанавливает исходный профиль в finally.
5. Архивировать `Logs/Capy67-idle-checks.txt`, `Logs/Capy67-layout.txt`, свежие `Logs/GardenReview/*.png`, время, разрешение, хеши и diff. Имена отчётов перезаписываются следующим запуском.

### Матрица

| Изменение | Проверка |
|---|---|
| Только агенты/документация | TOML, обязательные поля, ссылки и пути, обнаружение ролей, отсутствие неразрешённых установок |
| Модель, баланс, save | Check-Activities + staging compile + миграции + полный Unity |
| UI, декор, assets | Compile + hash protection + полный Unity + ручной просмотр мобильных кадров |
| Ввод, анимация | Предыдущее + rapid taps, interrupted gesture, focus/pause, calm, повторное закрытие/открытие |
| SDK/WebGL после возобновления | Свежая сборка, browser errors/network, touch, audio, background/resume, reload/save, SDK callbacks |

Мобильная матрица для будущих визуальных изменений: 320×568, 360×640, 390×844, 412×915, затем desktop resize. Harness сам разрешения не перебирает: QA/интегратор выбирает каждый размер и фиксирует доказательства. Проверять фактические touch targets, читаемость, границы панелей, safe areas, отмену drag, отсутствие input interception декором. Эмуляция не заменяет физический Android.

## Baseline на момент настройки команды

- `Verification/UnifiedGarden/compile.txt`: обе сборки компилируются.
- Сверены 278 существовавших ресурсных файлов; изменений нет (`resource-protection.txt`).
- Последний actual Unity report: **FAIL: long pet requests payment without a tap after 25122** (29 октября, 22:04 по журналу). Этот сбой сохранён в `Verification/AgentTeam/baseline-unity.txt`.
- Тест удержания ждёт 0.7 с, порог — 0.6 с; потеря фокуса вызывает Suspend/Release. Возможная причина — фокус или узкий запас времени, но это гипотеза, не доказанный диагноз. Первое поручение QA: воспроизвести и различить штатную отмену ввода, сбой механики и нестабильность теста. Не отключать проверку.
- Предыдущий PASS 30439 был до последней коррекции подписи категории; он не заменяет текущий FAIL.
- Известное прежнее визуальное покрытие — Unity 360×640. Другие viewport и физический Android в рамках настройки команды не проверялись: игровые файлы этой задачей не менялись.
- `Builds/Capy67WebGL` датирован 12 октября и устарел относительно текущих исходников. WebGL/публикация остаются на паузе.

## Skills и MCP

Проверены доступные в сессии capabilities и локальный `codex mcp list` без включения серверов. Списки сессии и CLI могут различаться; перед работой проверять фактическую доступность.

| Инструмент | Состояние и применение |
|---|---|
| imagegen | Доступный skill и встроенная генерация; декор/иконки по задаче, не перерисовка защищённого персонажа |
| computer-use | Доступен для Unity через штатный runtime; один владелец окна |
| OpenAI Docs | Доступен; проверка формата агентов и конфигурации |
| Plugin Management | Доступен; поиск/проверка/предложение установки |
| cua_repl / node_repl | Включены в локальной MCP-конфигурации; браузерные и скриптовые инструменты доступны в сессии |
| Browser | `browser@openai-bundled` уже установлен и включён по каталогу CLI; сначала использовать имеющуюся возможность |
| GitHub / Code Review | Capabilities видны в сессии; локальная запись code-review выключена. Не обещать доступ к репозиторию без фактической проверки |
| Sites | Установлен, но не нужен для переноса существующей Unity-игры на другой стек |
| Документы/таблицы/визуализация | Доступны для спецификаций/анализа, подключать по необходимости |
| Unity MCP / Playwright MCP | Отдельные серверы не обнаружены в проверенном локальном списке; автоматически не добавлены |

### Game Studio — предложение установки

После уточнения `openai/plugins/game-studio` найден официальный пакет **game-studio@openai-curated-remote**, версия **0.1.2**, издатель OpenAI. Проверены [manifest](https://raw.githubusercontent.com/openai/plugins/main/plugins/game-studio/.codex-plugin/plugin.json), [marketplace](https://raw.githubusercontent.com/openai/plugins/main/.agents/plugins/marketplace.json) и [главный skill](https://raw.githubusercontent.com/openai/plugins/main/plugins/game-studio/skills/game-studio/SKILL.md). В manifest заявлена MIT; отдельный текст LICENSE не подтверждён. Сначала пакет не был установлен и пользователю предложена установка. Последующая проверка 30 октября подтвердила **installed=true, user_enabled=true**, обязательных app-зависимостей нет. Установочные команды агентом не выполнялись.

Это набор skills/scripts, не Unity MCP. В нём есть Phaser/Three.js/R3F workflows, UI, sprite pipeline и playtesting. Использовать выборочно QA/декор/UI; не применять выбор Phaser по умолчанию к этой игре. [game-playtest](https://raw.githubusercontent.com/openai/plugins/main/plugins/game-studio/skills/game-playtest/SKILL.md) может использовать имеющийся браузерный инструмент. [sprite-pipeline](https://raw.githubusercontent.com/openai/plugins/main/plugins/game-studio/skills/sprite-pipeline/SKILL.md) обращается к imagegen, вспомогательные Python-скрипты требуют Pillow. Они не запускались; отдельная проверка версии/назначения нужна перед установкой недостающего Pillow. Не добавлять Phaser, Three.js или Rapier в Unity-проект ради этого плагина.

Проверены локальные SKILL.md установленного пакета. Связка ролей: QA → `game-playtest`; UI → `game-ui-frontend` с адаптацией принципов к uGUI; художник/аниматор → `sprite-pipeline` только для разрешённого декоративного арта после чтения skill. Главный `game-studio` используется для маршрутизации, не для смены движка. Предположения DOM/CSS из UI-skill не применяются: существующие CanvasScaler/BoardFit и композиция сохраняются. Роли programmer/economist/platform продолжают использовать C#, текущие проверки модели и PluginYG2.

### Браузерные тесты — рекомендуемое дополнение

Сначала использовать установленный Browser для доступного UI smoke-test. Для воспроизводимых тестов viewport/touch, console/network и traces рекомендован **официальный Playwright Microsoft**: [документация эмуляции](https://playwright.dev/docs/emulation), [репозиторий Playwright MCP](https://github.com/microsoft/playwright-mcp).

Предлагаемые пакеты: `@playwright/test` для повторяемых тестов и при необходимости `@playwright/mcp` для интерактивного агента. Это инструменты разработки, не runtime игры. В этой задаче они **не устанавливались**, MCP-конфигурация для их автоматического запуска не добавлялась. Перед установкой выбрать конкретный релиз, проверить publisher/repository, лицензию, зависимости, install scripts и lockfile; не использовать непроверенный `npx ...@latest`.

Для Unity canvas дерево доступности не раскрывает внутренние uGUI-кнопки: нужны скриншоты/координаты и проверка видимого результата; в MCP доступны opt-in vision capabilities. Использовать isolated context и loopback; проверку reload выполнять внутри того же тестового контекста, не в реальном профиле игрока. Проверка реальных SDK-реклам/платежей остаётся отдельной платформенной задачей. Предложение инструментов не снимает паузу WebGL и не разрешает публикацию.

## Приёмка настройки

Восемь TOML ролей, проектный config и AGENTS.md должны быть синтаксически корректны, роли уникальны, конфигурации в двух рабочих корнях совпадают. Нельзя добавлять скрытые hooks, установочные команды, секреты, новые MCP-серверы, модельные overrides или ослабление разрешений. Результат локальной проверки — `Verification/AgentTeam/config-validation.json`. Код игры и пользовательские сохранения этой задачей не изменяются.
