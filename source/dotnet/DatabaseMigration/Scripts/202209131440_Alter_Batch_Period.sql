﻿UPDATE Batch SET PeriodStart='2022-05-31T22:00:00' WHERE PeriodStart IS NULL
ALTER TABLE Batch
ALTER COLUMN PeriodStart DATETIME2 NOT NULL;

UPDATE Batch SET PeriodEnd='2022-06-01T22:00:00' WHERE PeriodEnd IS NULL
ALTER TABLE Batch
ALTER COLUMN PeriodEnd DATETIME2 NOT NULL;

GO 