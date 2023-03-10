BEGIN TRANSACTION;
DROP TABLE IF EXISTS "statuses";
CREATE TABLE IF NOT EXISTS "statuses" (
	"status"	TEXT NOT NULL,
	PRIMARY KEY("status")
);
DROP TABLE IF EXISTS "players";
CREATE TABLE IF NOT EXISTS "players" (
	"id"	INTEGER NOT NULL,
	"nickname"	TEXT UNIQUE,
	PRIMARY KEY("id" AUTOINCREMENT)
);
DROP TABLE IF EXISTS "sessions";
CREATE TABLE IF NOT EXISTS "sessions" (
	"id"	INTEGER NOT NULL,
	"idPlayer1"	INTEGER NOT NULL,
	"idPlayer2"	INTEGER,
	"field"	TEXT NOT NULL DEFAULT '.........',
	"status"	TEXT NOT NULL DEFAULT 'stepPlayer1',
	PRIMARY KEY("id"),
	FOREIGN KEY("idPlayer2") REFERENCES "players"("id"),
	FOREIGN KEY("status") REFERENCES "statuses"("status"),
	FOREIGN KEY("idPlayer1") REFERENCES "players"("id")
);
INSERT INTO "statuses" VALUES ('stepPlayer1'),
 ('stepPlayer2'),
 ('winPlayer1'),
 ('winPlayer2'),
 ('nobody');
INSERT INTO "players" VALUES (1,'Stalwitfen'),
 (3,'Ghost'),
 (4,'Kuplinov'),
 (5,'Grenadin'),
 (6,'Erok'),
 (7,'Ferdinand'),
 (8,'kesarev'),
 (9,'Portnoi'),
 (10,'Rat'),
 (11,'Fet'),
 (12,'Kukarachi');
INSERT INTO "sessions" VALUES (1,1,3,'.X0......','stepPlayer1'),
 (2,4,10,'..X..0..X','stepPlayer2'),
 (3,6,7,'.........','stepPlayer1'),
 (4,5,8,'X.0X0X0..','winPlayer2');
COMMIT;
