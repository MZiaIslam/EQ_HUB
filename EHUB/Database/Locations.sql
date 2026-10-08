-- Work locations (branches/campuses) and the staff <-> location assignments.
-- Run once against the EHUB database. Idempotent.

IF OBJECT_ID('dbo.tblWorkLocations') IS NULL
CREATE TABLE dbo.tblWorkLocations (
    LocationId   INT IDENTITY(1,1) PRIMARY KEY,
    LocationName NVARCHAR(150) NOT NULL,
    Address      NVARCHAR(300) NULL,
    City         NVARCHAR(100) NULL,
    Phone        NVARCHAR(50)  NULL,
    IsActive     BIT NOT NULL CONSTRAINT DF_tblWorkLocations_IsActive DEFAULT (1),
    IsDel        BIT NOT NULL CONSTRAINT DF_tblWorkLocations_IsDel DEFAULT (0),
    CreatedBy    INT NULL,
    CreatedOn    DATETIME NOT NULL CONSTRAINT DF_tblWorkLocations_CreatedOn DEFAULT (GETDATE()),
    ModifyBy     INT NULL,
    ModifyOn     DATETIME NULL
);
GO

IF OBJECT_ID('dbo.tblEmpWorkLocations') IS NULL
CREATE TABLE dbo.tblEmpWorkLocations (
    EmpID      INT NOT NULL,
    LocationId INT NOT NULL,
    CreatedBy  INT NULL,
    CreatedOn  DATETIME NOT NULL CONSTRAINT DF_tblEmpWorkLocations_CreatedOn DEFAULT (GETDATE()),
    CONSTRAINT PK_tblEmpWorkLocations PRIMARY KEY (EmpID, LocationId),
    CONSTRAINT FK_tblEmpWorkLocations_Loc FOREIGN KEY (LocationId) REFERENCES dbo.tblWorkLocations (LocationId)
);
GO

-- NOTE: the sidebar menu and page permissions are stored in the existing menu/page
-- tables behind usp_MMenu / usp_SMenu / usp_ModuleList. Register a page named
-- 'WorkLocations' (label "Locations", controller Administration) under
-- System Administration there, then grant the group access from User Groups.
