/* ---------------------------------------------------------------------------
   dbo.Country - one row per Correlates of War country code. Every other table
   has a foreign key back to this one, so it comes first in TableCatalog
   (created first, dropped last).
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE TABLE dbo.Country
(
    Ccode smallint      NOT NULL CONSTRAINT PK_Country PRIMARY KEY,
    Abbr  nvarchar(8)   NOT NULL,
    Name  nvarchar(100) NOT NULL
);
GO
