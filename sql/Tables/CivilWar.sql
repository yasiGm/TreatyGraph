/* ---------------------------------------------------------------------------
   dbo.CivilWar - civil / internationalised wars (COW Intra-State War Data).
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE TABLE dbo.CivilWar
(
    CivilWarId          int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CivilWar PRIMARY KEY,
    Ccode                smallint      NOT NULL,
    WarName              nvarchar(150) NOT NULL,
    StartYear            smallint      NOT NULL,
    StartMonth           tinyint       NULL,
    StartDay             tinyint       NULL,
    EndYear              smallint      NULL,
    EndMonth             tinyint       NULL,
    EndDay               tinyint       NULL,
    SortKey              int           NOT NULL,
    Sides                nvarchar(400) NOT NULL,
    IsInternationalized  bit           NOT NULL,
    CONSTRAINT FK_CivilWar_Country FOREIGN KEY (Ccode) REFERENCES dbo.Country (Ccode)
);
CREATE INDEX IX_CivilWar_Country ON dbo.CivilWar (Ccode, SortKey);
GO
