START TRANSACTION;
CREATE TABLE "ApplicationMobileAppLinks" (
    "Id" uuid NOT NULL,
    "ApplicationId" uuid NOT NULL,
    "AndroidPackageName" character varying(255),
    "AndroidSha256CertificateFingerprint" character varying(128),
    "IosBundleId" character varying(255),
    "IosTeamId" character varying(64),
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_ApplicationMobileAppLinks" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ApplicationMobileAppLinks_Applications_ApplicationId" FOREIGN KEY ("ApplicationId") REFERENCES "Applications" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_ApplicationMobileAppLinks_ApplicationId" ON "ApplicationMobileAppLinks" ("ApplicationId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260911010731_AddApplicationMobileAppLinks', '10.0.10');

COMMIT;

