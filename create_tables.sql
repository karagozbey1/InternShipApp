-- PostgreSQL veritabanýnda universiteler ve iller tablolarýný oluþtur
-- Bu script migration'larýn PostgreSQL'de çalýþmasý için gerekli tablolarý oluþturur

-- 1. Ýller tablosunu oluþtur
CREATE TABLE IF NOT EXISTS "iller" (
    "il_id" SERIAL PRIMARY KEY,
    "il_name" VARCHAR(255) NOT NULL,
    "status" INTEGER NOT NULL DEFAULT 1
);

-- 2. Üniversiteler tablosunu oluþtur
CREATE TABLE IF NOT EXISTS "universiteler" (
    "uni_id" SERIAL PRIMARY KEY,
    "uni_name" VARCHAR(255) NOT NULL,
    "status" SMALLINT NOT NULL DEFAULT 1,
    "il_id" INTEGER NOT NULL,
    CONSTRAINT "FK_universiteler_iller_il_id" 
        FOREIGN KEY ("il_id") 
        REFERENCES "iller"("il_id") 
        ON DELETE RESTRICT
);

-- 3. Index'leri oluþtur
CREATE INDEX IF NOT EXISTS "IX_universiteler_il_id" ON "universiteler" ("il_id");

-- 4. Users tablosuna UniversityId kolonu ekle (eðer yoksa)
DO $$ 
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'Users' AND column_name = 'UniversityId'
    ) THEN
        ALTER TABLE "Users" ADD COLUMN "UniversityId" INTEGER NULL;
    END IF;
END $$;

-- 5. Users tablosundaki UniversityId için foreign key oluþtur
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_Users_universiteler_UniversityId'
    ) THEN
        ALTER TABLE "Users" 
        ADD CONSTRAINT "FK_Users_universiteler_UniversityId" 
        FOREIGN KEY ("UniversityId") 
        REFERENCES "universiteler"("uni_id") 
        ON DELETE RESTRICT;
    END IF;
END $$;

-- 6. Users tablosundaki UniversityId için index oluþtur
CREATE INDEX IF NOT EXISTS "IX_Users_UniversityId" ON "Users" ("UniversityId");

-- 7. Örnek veri ekle (Türkiye'nin bazý illeri)
INSERT INTO "iller" ("il_name", "status") 
VALUES 
    ('Ýstanbul', 1),
    ('Ankara', 1),
    ('Ýzmir', 1),
    ('Bursa', 1),
    ('Antalya', 1),
    ('Adana', 1),
    ('Konya', 1),
    ('Gaziantep', 1),
    ('Kayseri', 1),
    ('Eskiþehir', 1)
ON CONFLICT DO NOTHING;

-- 8. Örnek üniversite verileri ekle
INSERT INTO "universiteler" ("uni_name", "il_id", "status")
SELECT 
    university_name,
    (SELECT "il_id" FROM "iller" WHERE "il_name" = city_name),
    1
FROM (VALUES
    ('Boðaziçi Üniversitesi', 'Ýstanbul'),
    ('Ýstanbul Teknik Üniversitesi', 'Ýstanbul'),
    ('Ýstanbul Üniversitesi', 'Ýstanbul'),
    ('Marmara Üniversitesi', 'Ýstanbul'),
    ('Yýldýz Teknik Üniversitesi', 'Ýstanbul'),
    ('Sabancý Üniversitesi', 'Ýstanbul'),
    ('Koç Üniversitesi', 'Ýstanbul'),
    ('Ankara Üniversitesi', 'Ankara'),
    ('Hacettepe Üniversitesi', 'Ankara'),
    ('ODTÜ (Orta Doðu Teknik Üniversitesi)', 'Ankara'),
    ('Gazi Üniversitesi', 'Ankara'),
    ('Bilkent Üniversitesi', 'Ankara'),
    ('Ege Üniversitesi', 'Ýzmir'),
    ('Dokuz Eylül Üniversitesi', 'Ýzmir'),
    ('Ýzmir Yüksek Teknoloji Enstitüsü', 'Ýzmir'),
    ('Uludað Üniversitesi', 'Bursa'),
    ('Akdeniz Üniversitesi', 'Antalya'),
    ('Çukurova Üniversitesi', 'Adana'),
    ('Selçuk Üniversitesi', 'Konya'),
    ('Gaziantep Üniversitesi', 'Gaziantep'),
    ('Erciyes Üniversitesi', 'Kayseri'),
    ('Eskiþehir Osmangazi Üniversitesi', 'Eskiþehir'),
    ('Anadolu Üniversitesi', 'Eskiþehir')
) AS universities(university_name, city_name)
WHERE (SELECT "il_id" FROM "iller" WHERE "il_name" = city_name) IS NOT NULL
ON CONFLICT DO NOTHING;

-- 9. Veritabaný migration durumunu kontrol et
-- Bu kýsým opsiyonel - EF Core migration tablosu mevcutsa güncellenir
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '__EFMigrationsHistory') THEN
        INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
        VALUES ('20250818134059_MigrationAddUniversityAndProvince', '9.0.0')
        ON CONFLICT ("MigrationId") DO NOTHING;
    END IF;
END $$;

-- Tablo durumunu kontrol etmek için sorgu
SELECT 
    'iller' as table_name, 
    COUNT(*) as record_count 
FROM "iller"
UNION ALL
SELECT 
    'universiteler' as table_name, 
    COUNT(*) as record_count 
FROM "universiteler";