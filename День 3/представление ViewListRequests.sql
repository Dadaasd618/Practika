-- =====================================================
-- ХРАНИМАЯ ПРОЦЕДУРА: FilteringRequests
-- ОПИСАНИЕ: Фильтрация заявок по типу, подразделению и статусу
-- =====================================================

CREATE OR REPLACE FUNCTION FilteringRequests(
    p_type VARCHAR DEFAULT NULL,
    p_department_id INT DEFAULT NULL,
    p_status VARCHAR DEFAULT NULL
)
RETURNS TABLE(
    request_id INT,
    type VARCHAR,
    start_date DATE,
    end_date DATE,
    status VARCHAR,
    rejection_reason TEXT,
    comment TEXT,
    created_at TIMESTAMP,
    department_name VARCHAR,
    purpose_name VARCHAR,
    user_email VARCHAR,
    visitor_full_name VARCHAR,
    visitor_passport VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        r.request_id,
        r.type,
        r.start_date,
        r.end_date,
        r.status,
        r.rejection_reason,
        r.comment,
        r.created_at,
        d.name AS department_name,
        vp.name AS purpose_name,
        u.email AS user_email,
        COALESCE(v.last_name || ' ' || v.first_name, 'Групповая заявка') AS visitor_full_name,
        v.passport_series || ' ' || v.passport_number AS visitor_passport
    FROM requests r
    LEFT JOIN departments d ON r.department_id = d.department_id
    LEFT JOIN visit_purposes vp ON r.purpose_id = vp.purpose_id
    LEFT JOIN users u ON r.user_id = u.user_id
    LEFT JOIN individual_requests ir ON r.request_id = ir.request_id
    LEFT JOIN visitors v ON ir.visitor_id = v.visitor_id
    WHERE (p_type IS NULL OR r.type = p_type)
      AND (p_department_id IS NULL OR r.department_id = p_department_id)
      AND (p_status IS NULL OR r.status = p_status)
    ORDER BY r.created_at DESC;
END;
$$ LANGUAGE plpgsql;

-- =====================================================
-- ПРИМЕРЫ ИСПОЛЬЗОВАНИЯ:
-- =====================================================

-- 1. Все заявки
-- SELECT * FROM FilteringRequests(NULL, NULL, NULL);

-- 2. Только личные заявки
-- SELECT * FROM FilteringRequests('individual', NULL, NULL);

-- 3. Только групповые заявки
-- SELECT * FROM FilteringRequests('group', NULL, NULL);

-- 4. Заявки по подразделению (например, department_id = 1)
-- SELECT * FROM FilteringRequests(NULL, 1, NULL);

-- 5. Заявки со статусом 'pending' (на проверке)
-- SELECT * FROM FilteringRequests(NULL, NULL, 'pending');

-- 6. Комбинированный фильтр: личные заявки, подразделение 1, статус 'approved'
-- SELECT * FROM FilteringRequests('individual', 1, 'approved');