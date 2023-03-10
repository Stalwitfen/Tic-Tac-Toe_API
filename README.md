# REST API для игры "Крестики-Нолики"



## Структура файла
- Список запросов
- Подробный разбор каждого запроса
- Описание базы данных



## Список запросов:


- GET: /api/session/{sessionId}
- GET: /api/session/{sessionId}/player2Id
- GET: /api/player/{playerId}/session
- POST: /api/player/{nickname}
- POST: /api/player/{playerId}/session
- PUT: /api/step

Формат сообщений - JSON.



## Подробный разбор каждого запроса:


### GET: /api/session/{sessionId}

Возвращает поле и статус сессии.

Пример запроса:
> /api/session/2

Пример ответа:

    {
        "value": {
            "field": ".........",
            "status": "stepPlayer1"
        },
        "formatters": [],
        "contentTypes": [],
        "declaredType": null,
        "statusCode": 200
    }
    
ответ содержит код статуса - 200 (Ok);

если указан несуществующий id:

    {
        "statusCode": 404
    }
    
ответ содержит код ошибки (404 - Not Found).


### GET: /api/session/{sessionId}/player2Id

Возвращает id второго игрока. Вынесено отдельно от session/{sessionId}, т.к. эти данные нужны только при ожидании второго  игрока, когда создана новая игра. Так же предполагается, что этот запрос будет повторяться многократно, поэтому экономится трафик.

Пример запроса:
> /api/session/4/player2Id

Пример ответа:

    {
        "value": 5,
        "formatters": [],
        "contentTypes": [],
        "declaredType": null,
        "statusCode": 200
    }
    
ответ содержит код статуса - 200 (Ok);

если второго игрока нет, value будет иметь значение null;

если указан несуществующий sessionId:

    {
        "statusCode": 404
    }

ответ содержит код ошибки (404 - Not Found).


### GET: /api/player/{playerId}/session

Возвращает последнюю сессию игрока (id сессии, номер игрока (первый/второй), id другого игрока). Предполагается, что одновременно игрок может играть только в одну партию.

Пример запроса:
> /api/player/1/session

Пример ответа:

    {
        "value": {
            "sessionId": 1,
            "playerNum": "first",
            "otherPlayerId": 3
        },
        "formatters": [],
        "contentTypes": [],
        "declaredType": null,
        "statusCode": 200
    }
    
ответ содержит код статуса - 200 (Ok);

если игрок создал новую игру, а второй игрок ещё не присоединился, то otherPlayerId будет иметь значение null;

если указан несуществующий id:

    {
        "statusCode": 404
    }

ответ содержит код ошибки (404 - Not Found).


### POST: /api/player/{nickname}

Создание нового игрока. Возвращает id созданного игрока.

Пример запроса:
> /api/player/Stalwitfen

Пример ответа:

    {
        "value": {
            "playerId": 1
        },
        "formatters": [],
        "contentTypes": [],
        "declaredType": null,
        "statusCode": 200
    }
    
ответ содержит код статуса - 200 (Ok);

если никнейм уже занят (или при возникновении других ошибок во время выполнения запроса к БД):

    {
        "statusCode": 400
    }
    
ответ содержит код ошибки (400 - Bad Request).


### POST: /api/player/{playerId}/session

Создание/присоединение к новой сессии (игры).

Пример запроса:
> /api/player/1/session

Пример ответа:

    {
        "location": "/api/session/1",
        "value": {
            "field": ".........",
            "status": "stepPlayer1",
            "playerNum": "first"
        },
        "formatters": [],
        "contentTypes": [],
        "declaredType": null,
        "statusCode": 201
    }
    
ответ содержит расположение созданного ресурса, поле и статус сессии, номер игрока, код статуса 201 (Created);

если у последней сессии нет второго игрока, то им становится игрок, отправивший этот запрос:

    {
        "value": {
            "sessionId": 5,
            "status": "stepPlayer1",
            "playerNum": "second"
        },
        "formatters": [],
        "contentTypes": [],
        "declaredType": null,
        "statusCode": 200
    }
    
ответ содержит id сессии (игры), к которой присоединился игрок, статус сессии, номер игрока, код статуса 200 (Ok);

если ошибка в выполнении запроса:

    {
        "statusCode": 400
    }
    
ответ содержит код ошибки (400 - Bad Request).


### PUT: /api/step

Ход игрока. Запрос отправляется вместе с телом (body), в котором содержится объект из 3-х пар ключ-значение: cellId, playerNum, sessionId, где
- cellId 		- число, адрес ячейки поля (от 0 по 8)
- playerNum 	- строка, номер игрока ("first" или "second")
- sessionId 	- число, id сессии

Пример тела запроса:

    {
        "CellId": 1,
        "PlayerNum": "first",
        "SessionId": 1
    }

Пример ответа:

    {
        "value": {
            "field": ".X.......",
            "status": "stepPlayer2"
        },
        "formatters": [],
        "contentTypes": [],
        "declaredType": null,
        "statusCode": 200
    }

ответ содержит измененное поле, новый статус сессии и код статуса - 200 (Ok);

если в запросе неверный адрес ячейки или при попытке записать значение в занятую ячейку

    {
        "value": {
            "errorMessage": "Invalid cellId"
        },
        "formatters": [],
        "contentTypes": [],
        "declaredType": null,
        "statusCode": 409
    }
    
ответ содержит сообщение ошибки и код статуса - 409 (Conflict);

если неверный номер игрока:

    {
        "value": {
            "errorMessage": "Invalid playerNum"
        },
        "formatters": [],
        "contentTypes": [],
        "declaredType": null,
        "statusCode": 409
    }
    
ответ содержит сообщение ошибки и код статуса - 409 (Conflict);

если указана несуществующая сессия или иная ошибка при обращении к БД:

    {
        "value": {
            "errorMessage": "Invalid sessionId"
        },
        "formatters": [],
        "contentTypes": [],
        "declaredType": null,
        "statusCode": 409
    }
    
ответ содержит сообщение ошибки и код статуса - 409 (Conflict);

если сессия уже завершена (имеет статус "winPlayer1", "winPlayer2" или "nobody"):

    {
        "value": {
            "errorMessage": "The session is already over",
            "sessionStatus": "winPlayer2"
        },
        "formatters": [],
        "contentTypes": [],
        "declaredType": null,
        "statusCode": 409
    }

ответ содержит сообщение ошибки, статус завершенной сессии и код статуса - 409 (Conflict);

если сейчас ход другого игрока:

    {
        "value": {
            "errorMessage": "Another player's move",
            "sessionStatus": "stepPlayer2"
        },
        "formatters": [],
        "contentTypes": [],
        "declaredType": null,
        "statusCode": 409
    }

ответ содержит сообщение ошибки, текущий статус сессии и код статуса - 409 (Conflict);

если по каким-то непредвиденным причинам возникнет ошибка при записи в БД:

    {
        "value": {
            "errorMessage": "UPDATE database error"
        },
        "formatters": [],
        "contentTypes": [],
        "declaredType": null,
        "statusCode": 409
    }
    
ответ содержит сообщение ошибки и код статуса - 409 (Conflict).



## Описание базы данных


Можно ознакомится со скриптом SQL, он добавлен в проект именно для этого (tic-tac-toe.sql).
БД хранится в файле, tic-tac-toe.db; СУБД - SQLite.

Таблицы:
- players
- statuses
- sessions


### Таблица players

Состоит из 2-х столбцов:
- id (INTEGER PK AI)
- nickname (TEXT NN UNIQUE)

Добавлено 11 записей.


### Таблица statuses

Состоит из 1-го столбца:
- status (TEXT PK NN)

Является вспомогательной, неизменной. Содержит 5 значений/строк (статусов сессии):
- stepPlayer1	- ход первого игрока
- stepPlayer2	- ход второго игрока
- winPlayer1	- победа первого игрока
- winPlayer2	- победа второго игрока
- nobody		- ничья


### Таблица sessions

Состоит из 5-ти столбцов:
- id (INTEGER PK NN AI)	
- idPlayer1	(INTEGER NN)	внешний ключ на таблицу players
- idPlayer2	(INTEGER)		внешний ключ на таблицу players
- field	(TEXT NN)			описание ниже; значение по умолчанию - "........."
- status (TEXT NN)			внешний ключ на таблицу statuses; значение по умолчанию - "stepPlayer1"

Поле field является строкой из 9 символов, где каждый символ имеет значение
- 'X' (большой латинский икс),
- '0' (цифра ноль),
- '.' - свободная ячейка поля.

Так выглядит игровое поле:

    0 1 2
    3 4 5
    6 7 8

где каждая цифра - это адрес ячейки (адрес в массиве символов(строке) соответственно).

Добавлено 4 записи.
