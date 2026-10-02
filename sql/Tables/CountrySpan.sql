/* ---------------------------------------------------------------------------
   dbo.CountrySpan - years in which the country is a member of the COW state
   system (a country can have several spans, e.g. a state that dissolved and
   later re-formed).
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE TABLE dbo.CountrySpan
(
    Ccode     smallint NOT NULL,
    StartYear smallint NOT NULL,
    EndYear   smallint NOT NULL,
    CONSTRAINT PK_CountrySpan PRIMARY KEY (Ccode, StartYear),
    CONSTRAINT FK_CountrySpan_Country FOREIGN KEY (Ccode) REFERENCES dbo.Country (Ccode)
);
GO
