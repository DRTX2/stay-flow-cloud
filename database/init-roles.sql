-- =============================================================================
-- StayFlow — PostgreSQL Role Initialization
-- =============================================================================
-- Run this script ONCE against your PostgreSQL/Neon database as a superuser
-- (e.g., the `neondb_owner` role in Neon, or `postgres` locally) before
-- running any migrations.
--
-- Security model:
--   stayflow_migrator   DDL-level: can CREATE/ALTER/DROP tables and indexes.
--                       Used exclusively by StayFlow.MigrationHost.
--
--   stayflow_app        DML-only: SELECT, INSERT, UPDATE, DELETE on app tables.
--                       Used by the running API. Cannot alter the schema.
--
-- Usage (local dev):
--   psql -U postgres -d stayflow -f database/init-roles.sql
--
-- Usage (Neon — via SQL Editor in the Neon dashboard or psql):
--   \i database/init-roles.sql
--
-- After running this script, set the following environment variables:
--   STAYFLOW_MIGRATOR_CONNECTION=Host=...;Username=stayflow_migrator;Password=<migrator_pwd>
--   ConnectionStrings__Default=Host=...;Username=stayflow_app;Password=<app_pwd>
-- =============================================================================

-- ─── 1. Create roles ────────────────────────────────────────────────────────

-- Replace '<migrator_password>' and '<app_password>' with strong secrets.
-- In Neon: use the Neon dashboard to create roles with passwords, then grant
-- the permissions below.

DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'stayflow_migrator') THEN
        CREATE ROLE stayflow_migrator LOGIN PASSWORD '<migrator_password>';
        RAISE NOTICE 'Role stayflow_migrator created.';
    ELSE
        RAISE NOTICE 'Role stayflow_migrator already exists, skipping creation.';
    END IF;
END
$$;

DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'stayflow_app') THEN
        CREATE ROLE stayflow_app LOGIN PASSWORD '<app_password>';
        RAISE NOTICE 'Role stayflow_app created.';
    ELSE
        RAISE NOTICE 'Role stayflow_app already exists, skipping creation.';
    END IF;
END
$$;

-- ─── 2. Schema ownership: migrator owns the schema ──────────────────────────

-- Grant migrator the ability to create and alter objects in the public schema.
GRANT ALL PRIVILEGES ON SCHEMA public TO stayflow_migrator;

-- ─── 3. App user: restricted to DML only ────────────────────────────────────

-- Allow the app user to use the schema (required to see tables).
GRANT USAGE ON SCHEMA public TO stayflow_app;

-- Grant DML on all existing tables.
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO stayflow_app;

-- Grant USAGE on all sequences (needed for SERIAL / IDENTITY columns).
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO stayflow_app;

-- Ensure future tables created by the migrator also grant DML to the app user.
ALTER DEFAULT PRIVILEGES FOR ROLE stayflow_migrator IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO stayflow_app;

ALTER DEFAULT PRIVILEGES FOR ROLE stayflow_migrator IN SCHEMA public
    GRANT USAGE, SELECT ON SEQUENCES TO stayflow_app;

-- ─── 4. Safety: explicitly deny DDL to the app user ─────────────────────────

-- Revoke the ability to create objects in the schema from the app user.
-- This is the default in PostgreSQL, but we make it explicit.
REVOKE CREATE ON SCHEMA public FROM stayflow_app;

-- ─── 5. Verify ──────────────────────────────────────────────────────────────

SELECT
    rolname,
    rolcanlogin,
    rolcreatedb,
    rolcreaterole,
    rolsuper
FROM pg_roles
WHERE rolname IN ('stayflow_migrator', 'stayflow_app')
ORDER BY rolname;
