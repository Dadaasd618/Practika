-- =====================================================
-- 1. Добавляем колонку login, если её нет
-- =====================================================
ALTER TABLE visitors ADD COLUMN IF NOT EXISTS login VARCHAR(100) UNIQUE;

-- =====================================================
-- 2. Удаляем старый триггер и функцию (если есть)
-- =====================================================
DROP TRIGGER IF EXISTS trg_visitor_login ON visitors;
DROP FUNCTION IF EXISTS generate_visitor_login();

-- =====================================================
-- 3. Создаём функцию для триггера
-- =====================================================
CREATE OR REPLACE FUNCTION generate_visitor_login()
RETURNS TRIGGER AS $$
DECLARE
    base_login VARCHAR(100);
    final_login VARCHAR(100);
    counter INT := 0;
BEGIN
    -- Берём часть email до @
    base_login := SPLIT_PART(NEW.email, '@', 1);
    
    -- Удаляем недопустимые символы (оставляем только буквы, цифры, подчёркивание)
    base_login := regexp_replace(base_login, '[^a-zA-Z0-9_]', '', 'g');
    
    -- Если получилось пусто, используем 'user'
    IF base_login = '' THEN
        base_login := 'user';
    END IF;
    
    -- Проверяем уникальность
    final_login := base_login;
    
    WHILE EXISTS (SELECT 1 FROM visitors WHERE login = final_login) LOOP
        counter := counter + 1;
        final_login := base_login || counter;
    END LOOP;
    
    -- Устанавливаем логин
    NEW.login := final_login;
    
    -- Эмуляция отправки сообщения
    RAISE NOTICE 'Сообщение пользователю: Ваш логин: %', final_login;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- 4. Создаём триггер
-- =====================================================
CREATE TRIGGER trg_visitor_login
    BEFORE INSERT ON visitors
    FOR EACH ROW
    EXECUTE FUNCTION generate_visitor_login();

-- =====================================================
-- 5. Тестируем (вставка нового посетителя)
-- =====================================================
INSERT INTO visitors (last_name, first_name, email, birth_date, passport_series, passport_number, passport_scan_path)
VALUES ('Тестов', 'Тест', 'testuser@example.com', '1990-01-01', '1234', '567890', '/scans/test.pdf');

-- =====================================================
-- 6. Проверяем результат
-- =====================================================
SELECT visitor_id, last_name, first_name, email, login FROM visitors WHERE email = 'testuser@example.com';

-- =====================================================
-- 7. Способ 2: Функция для ручной генерации логина
-- =====================================================
CREATE OR REPLACE FUNCTION generate_login_for_visitor(p_visitor_id INT)
RETURNS VARCHAR AS $$
DECLARE
    v_email VARCHAR(100);
    base_login VARCHAR(100);
    final_login VARCHAR(100);
    counter INT := 0;
BEGIN
    SELECT email INTO v_email FROM visitors WHERE visitor_id = p_visitor_id;
    
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Посетитель с ID % не найден', p_visitor_id;
    END IF;
    
    base_login := SPLIT_PART(v_email, '@', 1);
    base_login := regexp_replace(base_login, '[^a-zA-Z0-9_]', '', 'g');
    
    IF base_login = '' THEN
        base_login := 'user';
    END IF;
    
    final_login := base_login;
    
    WHILE EXISTS (SELECT 1 FROM visitors WHERE login = final_login AND visitor_id != p_visitor_id) LOOP
        counter := counter + 1;
        final_login := base_login || counter;
    END LOOP;
    
    UPDATE visitors SET login = final_login WHERE visitor_id = p_visitor_id;
    
    RAISE NOTICE 'Сообщение пользователю: Ваш логин: %', final_login;
    
    RETURN final_login;
END;
$$ LANGUAGE plpgsql;