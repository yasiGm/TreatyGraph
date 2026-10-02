/* ---------------------------------------------------------------------------
   dbo.Leader - effective leaders (Archigos 4.1). Loaded for the pilot
   countries only (see data/leaders_pilot.csv).
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE TABLE dbo.Leader
(
    LeaderId  int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Leader PRIMARY KEY,
    Ccode     smallint      NOT NULL,
    Name      nvarchar(120) NOT NULL,
    StartDate date          NOT NULL,
    EndDate   date          NOT NULL,
    ExitType  nvarchar(60)  NOT NULL,
    CONSTRAINT FK_Leader_Country FOREIGN KEY (Ccode) REFERENCES dbo.Country (Ccode)
);
CREATE INDEX IX_Leader_Country ON dbo.Leader (Ccode, StartDate);
GO
