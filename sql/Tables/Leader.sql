/* ---------------------------------------------------------------------------
   dbo.Leader - terms in office: heads of state and of government (Wikidata) with Archigos 4.1
   filling the years Wikidata does not cover (see data/raw/leaders/leaders_combined.csv).
   Role: 'state', 'government' or 'both'.
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
    Role      nvarchar(12)  NOT NULL,
    Source    nvarchar(30)  NOT NULL,
    CONSTRAINT FK_Leader_Country FOREIGN KEY (Ccode) REFERENCES dbo.Country (Ccode)
);
CREATE INDEX IX_Leader_Country ON dbo.Leader (Ccode, StartDate);
GO
