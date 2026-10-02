/* ---------------------------------------------------------------------------
   dbo.DyadEvent - everything that happened between a pair of countries
   (alliances, wars, diplomacy). CcodeLo < CcodeHi always, so a pair has
   exactly one canonical key. Dates keep the precision of the source: month /
   day are NULL when only the year is known.
   --------------------------------------------------------------------------- */
SET NOCOUNT ON;
GO

CREATE TABLE dbo.DyadEvent
(
    EventId        bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_DyadEvent PRIMARY KEY,
    CcodeLo        smallint      NOT NULL,
    CcodeHi        smallint      NOT NULL,
    EventType      varchar(12)   NOT NULL,   -- alliance | war | coalition | diplomacy
    Source         varchar(40)   NOT NULL,
    StartYear      smallint      NOT NULL,
    StartMonth     tinyint       NULL,
    StartDay       tinyint       NULL,
    EndYear        smallint      NULL,
    EndMonth       tinyint       NULL,
    EndDay         tinyint       NULL,
    IsOngoing      bit           NOT NULL,   -- still open when the source dataset ended
    IsLeftCensored bit           NOT NULL,   -- started on or before the first year covered
    SortKey        int           NOT NULL,   -- yyyymmdd of the start (missing parts = earliest)
    Title          nvarchar(200) NOT NULL,
    Detail         nvarchar(1000) NOT NULL,
    CONSTRAINT CK_DyadEvent_Order CHECK (CcodeLo < CcodeHi),
    CONSTRAINT CK_DyadEvent_Type CHECK (EventType IN ('alliance', 'war', 'coalition', 'diplomacy')),
    CONSTRAINT FK_DyadEvent_Lo FOREIGN KEY (CcodeLo) REFERENCES dbo.Country (Ccode),
    CONSTRAINT FK_DyadEvent_Hi FOREIGN KEY (CcodeHi) REFERENCES dbo.Country (Ccode)
);
CREATE INDEX IX_DyadEvent_Pair ON dbo.DyadEvent (CcodeLo, CcodeHi, SortKey) INCLUDE (EventType, Source);
CREATE INDEX IX_DyadEvent_Hi   ON dbo.DyadEvent (CcodeHi, EventType, SortKey);
GO
