/* ---------------------------------------------------------------------------
   dbo.CountryWar - a country's participation in an inter-state war (used for
   the country profile page: wins, losses, opponents, allies).
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE TABLE dbo.CountryWar
(
    CountryWarId int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CountryWar PRIMARY KEY,
    Ccode        smallint       NOT NULL,
    WarName      nvarchar(150)  NOT NULL,
    StartYear    smallint       NOT NULL,
    StartMonth   tinyint        NULL,
    StartDay     tinyint        NULL,
    EndYear      smallint       NOT NULL,
    EndMonth     tinyint        NULL,
    EndDay       tinyint        NULL,
    SortKey      int            NOT NULL,
    Result       varchar(12)    NULL,          -- winner | loser | compromise | stalemate
    IsInitiator  bit            NOT NULL,
    Opponents    nvarchar(1000) NOT NULL,      -- '; '-separated country names
    Allies       nvarchar(1000) NOT NULL,
    BattleDeaths int            NULL,
    CONSTRAINT FK_CountryWar_Country FOREIGN KEY (Ccode) REFERENCES dbo.Country (Ccode)
);
CREATE INDEX IX_CountryWar_Country ON dbo.CountryWar (Ccode, SortKey);
GO
