--
-- PostgreSQL database dump
--

\restrict eYxZLsdowPvicZxqwfALPGb3RX7jugXYPKQ347ataXdNVC0hgsrljmhPjY8W6dg

-- Dumped from database version 16.13
-- Dumped by pg_dump version 18.1

-- Started on 2026-04-19 19:38:22

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 4 (class 2615 OID 2200)
-- Name: public; Type: SCHEMA; Schema: -; Owner: pg_database_owner
--

CREATE SCHEMA public;


ALTER SCHEMA public OWNER TO pg_database_owner;

--
-- TOC entry 5044 (class 0 OID 0)
-- Dependencies: 4
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: pg_database_owner
--

COMMENT ON SCHEMA public IS 'standard public schema';


--
-- TOC entry 252 (class 1255 OID 16637)
-- Name: filteringrequests(character varying, integer, character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.filteringrequests(p_type character varying DEFAULT NULL::character varying, p_department_id integer DEFAULT NULL::integer, p_status character varying DEFAULT NULL::character varying) RETURNS TABLE(request_id integer, type character varying, start_date date, end_date date, status character varying, rejection_reason text, comment text, created_at timestamp without time zone, department_name character varying, purpose_name character varying, user_email character varying, visitor_full_name character varying, visitor_passport character varying)
    LANGUAGE plpgsql
    AS '
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
        COALESCE(v.last_name || '' '' || v.first_name, ''Групповая заявка'') AS visitor_full_name,
        v.passport_series || '' '' || v.passport_number AS visitor_passport
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
';


ALTER FUNCTION public.filteringrequests(p_type character varying, p_department_id integer, p_status character varying) OWNER TO postgres;

--
-- TOC entry 254 (class 1255 OID 16645)
-- Name: generate_login_for_visitor(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.generate_login_for_visitor(p_visitor_id integer) RETURNS character varying
    LANGUAGE plpgsql
    AS '
DECLARE
    v_email VARCHAR(100);
    base_login VARCHAR(100);
    final_login VARCHAR(100);
    counter INT := 0;
BEGIN
    SELECT email INTO v_email FROM visitors WHERE visitor_id = p_visitor_id;
    
    IF NOT FOUND THEN
        RAISE EXCEPTION ''Посетитель с ID % не найден'', p_visitor_id;
    END IF;
    
    base_login := SPLIT_PART(v_email, ''@'', 1);
    base_login := regexp_replace(base_login, ''[^a-zA-Z0-9_]'', '''', ''g'');
    
    IF base_login = '''' THEN
        base_login := ''user'';
    END IF;
    
    final_login := base_login;
    
    WHILE EXISTS (SELECT 1 FROM visitors WHERE login = final_login AND visitor_id != p_visitor_id) LOOP
        counter := counter + 1;
        final_login := base_login || counter;
    END LOOP;
    
    UPDATE visitors SET login = final_login WHERE visitor_id = p_visitor_id;
    
    RAISE NOTICE ''Сообщение пользователю: Ваш логин: %'', final_login;
    
    RETURN final_login;
END;
';


ALTER FUNCTION public.generate_login_for_visitor(p_visitor_id integer) OWNER TO postgres;

--
-- TOC entry 253 (class 1255 OID 16643)
-- Name: generate_visitor_login(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.generate_visitor_login() RETURNS trigger
    LANGUAGE plpgsql
    AS '
DECLARE
    base_login VARCHAR(100);
    final_login VARCHAR(100);
    counter INT := 0;
BEGIN
    -- Берём часть email до @
    base_login := SPLIT_PART(NEW.email, ''@'', 1);
    
    -- Удаляем недопустимые символы (оставляем только буквы, цифры, подчёркивание)
    base_login := regexp_replace(base_login, ''[^a-zA-Z0-9_]'', '''', ''g'');
    
    -- Если получилось пусто, используем ''user''
    IF base_login = '''' THEN
        base_login := ''user'';
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
    RAISE NOTICE ''Сообщение пользователю: Ваш логин: %'', final_login;
    
    RETURN NEW;
END;
';


ALTER FUNCTION public.generate_visitor_login() OWNER TO postgres;

--
-- TOC entry 239 (class 1255 OID 16620)
-- Name: login_user(character varying, character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.login_user(p_email character varying, p_password character varying) RETURNS integer
    LANGUAGE plpgsql
    AS '
DECLARE
    v_user_id INTEGER;
BEGIN
    SELECT user_id INTO v_user_id
    FROM users
    WHERE email = p_email AND password_hash = MD5(p_password);
    
    RETURN COALESCE(v_user_id, -1);
END;
';


ALTER FUNCTION public.login_user(p_email character varying, p_password character varying) OWNER TO postgres;

--
-- TOC entry 240 (class 1255 OID 16621)
-- Name: register_user(character varying, character varying); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.register_user(p_email character varying, p_password character varying) RETURNS boolean
    LANGUAGE plpgsql
    AS '
BEGIN
    INSERT INTO users (email, password_hash, role, registered_at) 
    VALUES (p_email, MD5(p_password), ''user'', NOW());
    RETURN TRUE;
EXCEPTION 
    WHEN unique_violation THEN
        RETURN FALSE;
END;
';


ALTER FUNCTION public.register_user(p_email character varying, p_password character varying) OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 238 (class 1259 OID 16647)
-- Name: blacklist; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.blacklist (
    blacklist_id integer NOT NULL,
    passport_number character varying(20) NOT NULL,
    full_name character varying(150) NOT NULL,
    reason text,
    added_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);


ALTER TABLE public.blacklist OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 16646)
-- Name: blacklist_blacklist_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.blacklist_blacklist_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.blacklist_blacklist_id_seq OWNER TO postgres;

--
-- TOC entry 5045 (class 0 OID 0)
-- Dependencies: 237
-- Name: blacklist_blacklist_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.blacklist_blacklist_id_seq OWNED BY public.blacklist.blacklist_id;


--
-- TOC entry 220 (class 1259 OID 16420)
-- Name: departments; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.departments (
    department_id integer NOT NULL,
    name character varying(100) NOT NULL
);


ALTER TABLE public.departments OWNER TO postgres;

--
-- TOC entry 219 (class 1259 OID 16419)
-- Name: departments_department_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.departments_department_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.departments_department_id_seq OWNER TO postgres;

--
-- TOC entry 5046 (class 0 OID 0)
-- Dependencies: 219
-- Name: departments_department_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.departments_department_id_seq OWNED BY public.departments.department_id;


--
-- TOC entry 221 (class 1259 OID 16426)
-- Name: employees; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.employees (
    employee_id integer NOT NULL,
    full_name character varying(150) NOT NULL,
    department_id integer,
    section character varying(100)
);


ALTER TABLE public.employees OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 16512)
-- Name: group_members; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.group_members (
    member_id integer NOT NULL,
    group_id integer NOT NULL,
    visitor_id integer NOT NULL,
    row_number integer NOT NULL
);


ALTER TABLE public.group_members OWNER TO postgres;

--
-- TOC entry 230 (class 1259 OID 16511)
-- Name: group_members_member_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.group_members_member_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.group_members_member_id_seq OWNER TO postgres;

--
-- TOC entry 5047 (class 0 OID 0)
-- Dependencies: 230
-- Name: group_members_member_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.group_members_member_id_seq OWNED BY public.group_members.member_id;


--
-- TOC entry 229 (class 1259 OID 16496)
-- Name: group_requests; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.group_requests (
    group_id integer NOT NULL,
    request_id integer NOT NULL,
    template_file_path character varying(500),
    photos_archive_path character varying(500)
);


ALTER TABLE public.group_requests OWNER TO postgres;

--
-- TOC entry 228 (class 1259 OID 16495)
-- Name: group_requests_group_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.group_requests_group_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.group_requests_group_id_seq OWNER TO postgres;

--
-- TOC entry 5048 (class 0 OID 0)
-- Dependencies: 228
-- Name: group_requests_group_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.group_requests_group_id_seq OWNED BY public.group_requests.group_id;


--
-- TOC entry 227 (class 1259 OID 16477)
-- Name: individual_requests; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.individual_requests (
    individual_id integer NOT NULL,
    request_id integer NOT NULL,
    visitor_id integer NOT NULL
);


ALTER TABLE public.individual_requests OWNER TO postgres;

--
-- TOC entry 226 (class 1259 OID 16476)
-- Name: individual_requests_individual_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.individual_requests_individual_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.individual_requests_individual_id_seq OWNER TO postgres;

--
-- TOC entry 5049 (class 0 OID 0)
-- Dependencies: 226
-- Name: individual_requests_individual_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.individual_requests_individual_id_seq OWNED BY public.individual_requests.individual_id;


--
-- TOC entry 233 (class 1259 OID 16529)
-- Name: passes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.passes (
    pass_id integer NOT NULL,
    request_id integer NOT NULL,
    issued_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    issued_by integer NOT NULL,
    entry_time timestamp without time zone,
    exit_time timestamp without time zone
);


ALTER TABLE public.passes OWNER TO postgres;

--
-- TOC entry 232 (class 1259 OID 16528)
-- Name: passes_pass_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.passes_pass_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.passes_pass_id_seq OWNER TO postgres;

--
-- TOC entry 5050 (class 0 OID 0)
-- Dependencies: 232
-- Name: passes_pass_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.passes_pass_id_seq OWNED BY public.passes.pass_id;


--
-- TOC entry 225 (class 1259 OID 16444)
-- Name: requests; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.requests (
    request_id integer NOT NULL,
    user_id integer NOT NULL,
    type character varying(20) NOT NULL,
    start_date date NOT NULL,
    end_date date NOT NULL,
    purpose_id integer NOT NULL,
    department_id integer NOT NULL,
    employee_id integer NOT NULL,
    status character varying(20) DEFAULT 'pending'::character varying,
    rejection_reason text,
    comment text NOT NULL,
    created_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT requests_status_check CHECK (((status)::text = ANY ((ARRAY['pending'::character varying, 'approved'::character varying, 'rejected'::character varying])::text[]))),
    CONSTRAINT requests_type_check CHECK (((type)::text = ANY ((ARRAY['individual'::character varying, 'group'::character varying])::text[])))
);


ALTER TABLE public.requests OWNER TO postgres;

--
-- TOC entry 224 (class 1259 OID 16443)
-- Name: requests_request_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.requests_request_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.requests_request_id_seq OWNER TO postgres;

--
-- TOC entry 5051 (class 0 OID 0)
-- Dependencies: 224
-- Name: requests_request_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.requests_request_id_seq OWNED BY public.requests.request_id;


--
-- TOC entry 216 (class 1259 OID 16400)
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    user_id integer NOT NULL,
    email character varying(100) NOT NULL,
    password_hash character varying(32) NOT NULL,
    registered_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    role character varying(50) DEFAULT 'user'::character varying
);


ALTER TABLE public.users OWNER TO postgres;

--
-- TOC entry 215 (class 1259 OID 16399)
-- Name: users_user_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.users_user_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.users_user_id_seq OWNER TO postgres;

--
-- TOC entry 5052 (class 0 OID 0)
-- Dependencies: 215
-- Name: users_user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_user_id_seq OWNED BY public.users.user_id;


--
-- TOC entry 223 (class 1259 OID 16437)
-- Name: visit_purposes; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.visit_purposes (
    purpose_id integer NOT NULL,
    name character varying(100) NOT NULL
);


ALTER TABLE public.visit_purposes OWNER TO postgres;

--
-- TOC entry 218 (class 1259 OID 16411)
-- Name: visitors; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.visitors (
    visitor_id integer NOT NULL,
    last_name character varying(50) NOT NULL,
    first_name character varying(50) NOT NULL,
    middle_name character varying(50),
    phone character varying(20),
    email character varying(100) NOT NULL,
    organization character varying(200),
    birth_date date NOT NULL,
    passport_series character varying(4) NOT NULL,
    passport_number character varying(6) NOT NULL,
    photo_path character varying(500),
    passport_scan_path character varying(500) NOT NULL,
    login character varying(100)
);


ALTER TABLE public.visitors OWNER TO postgres;

--
-- TOC entry 236 (class 1259 OID 16632)
-- Name: viewlistrequests; Type: VIEW; Schema: public; Owner: postgres
--

CREATE VIEW public.viewlistrequests AS
 SELECT r.request_id,
    r.type,
    r.start_date,
    r.end_date,
    r.status,
    r.rejection_reason,
    r.comment,
    r.created_at,
    u.user_id,
    u.email AS user_email,
    u.role AS user_role,
    d.department_id,
    d.name AS department_name,
    e.employee_id,
    e.full_name AS employee_name,
    e.section AS employee_section,
    vp.purpose_id,
    vp.name AS purpose_name,
    v.visitor_id,
    v.last_name,
    v.first_name,
    v.middle_name,
    v.phone,
    v.email AS visitor_email,
    v.organization,
    v.birth_date,
    v.passport_series,
    v.passport_number,
    v.photo_path,
    v.passport_scan_path,
    gr.group_id,
    gr.template_file_path,
    gr.photos_archive_path
   FROM (((((((public.requests r
     LEFT JOIN public.users u ON ((r.user_id = u.user_id)))
     LEFT JOIN public.departments d ON ((r.department_id = d.department_id)))
     LEFT JOIN public.employees e ON ((r.employee_id = e.employee_id)))
     LEFT JOIN public.visit_purposes vp ON ((r.purpose_id = vp.purpose_id)))
     LEFT JOIN public.individual_requests ir ON ((r.request_id = ir.request_id)))
     LEFT JOIN public.visitors v ON ((ir.visitor_id = v.visitor_id)))
     LEFT JOIN public.group_requests gr ON ((r.request_id = gr.request_id)));


ALTER VIEW public.viewlistrequests OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 16549)
-- Name: visit_logs; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.visit_logs (
    log_id integer NOT NULL,
    pass_id integer NOT NULL,
    recorded_by integer NOT NULL,
    recorded_at timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    comment text
);


ALTER TABLE public.visit_logs OWNER TO postgres;

--
-- TOC entry 234 (class 1259 OID 16548)
-- Name: visit_logs_log_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.visit_logs_log_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.visit_logs_log_id_seq OWNER TO postgres;

--
-- TOC entry 5053 (class 0 OID 0)
-- Dependencies: 234
-- Name: visit_logs_log_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.visit_logs_log_id_seq OWNED BY public.visit_logs.log_id;


--
-- TOC entry 222 (class 1259 OID 16436)
-- Name: visit_purposes_purpose_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.visit_purposes_purpose_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.visit_purposes_purpose_id_seq OWNER TO postgres;

--
-- TOC entry 5054 (class 0 OID 0)
-- Dependencies: 222
-- Name: visit_purposes_purpose_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.visit_purposes_purpose_id_seq OWNED BY public.visit_purposes.purpose_id;


--
-- TOC entry 217 (class 1259 OID 16410)
-- Name: visitors_visitor_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.visitors_visitor_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER SEQUENCE public.visitors_visitor_id_seq OWNER TO postgres;

--
-- TOC entry 5055 (class 0 OID 0)
-- Dependencies: 217
-- Name: visitors_visitor_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.visitors_visitor_id_seq OWNED BY public.visitors.visitor_id;


--
-- TOC entry 4814 (class 2604 OID 16650)
-- Name: blacklist blacklist_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.blacklist ALTER COLUMN blacklist_id SET DEFAULT nextval('public.blacklist_blacklist_id_seq'::regclass);


--
-- TOC entry 4802 (class 2604 OID 16423)
-- Name: departments department_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.departments ALTER COLUMN department_id SET DEFAULT nextval('public.departments_department_id_seq'::regclass);


--
-- TOC entry 4809 (class 2604 OID 16515)
-- Name: group_members member_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_members ALTER COLUMN member_id SET DEFAULT nextval('public.group_members_member_id_seq'::regclass);


--
-- TOC entry 4808 (class 2604 OID 16499)
-- Name: group_requests group_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_requests ALTER COLUMN group_id SET DEFAULT nextval('public.group_requests_group_id_seq'::regclass);


--
-- TOC entry 4807 (class 2604 OID 16480)
-- Name: individual_requests individual_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.individual_requests ALTER COLUMN individual_id SET DEFAULT nextval('public.individual_requests_individual_id_seq'::regclass);


--
-- TOC entry 4810 (class 2604 OID 16532)
-- Name: passes pass_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.passes ALTER COLUMN pass_id SET DEFAULT nextval('public.passes_pass_id_seq'::regclass);


--
-- TOC entry 4804 (class 2604 OID 16447)
-- Name: requests request_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests ALTER COLUMN request_id SET DEFAULT nextval('public.requests_request_id_seq'::regclass);


--
-- TOC entry 4798 (class 2604 OID 16403)
-- Name: users user_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN user_id SET DEFAULT nextval('public.users_user_id_seq'::regclass);


--
-- TOC entry 4812 (class 2604 OID 16552)
-- Name: visit_logs log_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_logs ALTER COLUMN log_id SET DEFAULT nextval('public.visit_logs_log_id_seq'::regclass);


--
-- TOC entry 4803 (class 2604 OID 16440)
-- Name: visit_purposes purpose_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_purposes ALTER COLUMN purpose_id SET DEFAULT nextval('public.visit_purposes_purpose_id_seq'::regclass);


--
-- TOC entry 4801 (class 2604 OID 16414)
-- Name: visitors visitor_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visitors ALTER COLUMN visitor_id SET DEFAULT nextval('public.visitors_visitor_id_seq'::regclass);


--
-- TOC entry 5038 (class 0 OID 16647)
-- Dependencies: 238
-- Data for Name: blacklist; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.blacklist (blacklist_id, passport_number, full_name, reason, added_at) VALUES (1, '0208 530509', 'Степанова Радинка', 'опрорп', '2026-04-18 09:12:47.415426');


--
-- TOC entry 5021 (class 0 OID 16420)
-- Dependencies: 220
-- Data for Name: departments; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.departments (department_id, name) VALUES (1, 'Производство');
INSERT INTO public.departments (department_id, name) VALUES (2, 'Сбыт');
INSERT INTO public.departments (department_id, name) VALUES (3, 'Администрация');
INSERT INTO public.departments (department_id, name) VALUES (4, 'Служба безопасности');
INSERT INTO public.departments (department_id, name) VALUES (5, 'Планирование');


--
-- TOC entry 5022 (class 0 OID 16426)
-- Dependencies: 221
-- Data for Name: employees; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.employees (employee_id, full_name, department_id, section) VALUES (9367788, 'Фомичева Авдотья Трофимовна', 1, NULL);
INSERT INTO public.employees (employee_id, full_name, department_id, section) VALUES (9788737, 'Гаврилова Римма Ефимовна', 2, NULL);
INSERT INTO public.employees (employee_id, full_name, department_id, section) VALUES (9736379, 'Носкова Наталия Прохоровна', 3, NULL);
INSERT INTO public.employees (employee_id, full_name, department_id, section) VALUES (9362832, 'Архипов Тимофей Васильевич', 4, NULL);
INSERT INTO public.employees (employee_id, full_name, department_id, section) VALUES (9737848, 'Орехова Вероника Артемовна', 5, NULL);
INSERT INTO public.employees (employee_id, full_name, department_id, section) VALUES (9768239, 'Савельев Павел Степанович', NULL, 'Общий отдел');
INSERT INTO public.employees (employee_id, full_name, department_id, section) VALUES (9404040, 'Чернов Всеволод Наумович', NULL, 'Охрана');


--
-- TOC entry 5032 (class 0 OID 16512)
-- Dependencies: 231
-- Data for Name: group_members; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.group_members (member_id, group_id, visitor_id, row_number) VALUES (1, 1, 1, 1);
INSERT INTO public.group_members (member_id, group_id, visitor_id, row_number) VALUES (2, 1, 2, 2);
INSERT INTO public.group_members (member_id, group_id, visitor_id, row_number) VALUES (3, 1, 3, 3);
INSERT INTO public.group_members (member_id, group_id, visitor_id, row_number) VALUES (4, 1, 4, 4);
INSERT INTO public.group_members (member_id, group_id, visitor_id, row_number) VALUES (5, 1, 5, 5);
INSERT INTO public.group_members (member_id, group_id, visitor_id, row_number) VALUES (6, 3, 8, 1);
INSERT INTO public.group_members (member_id, group_id, visitor_id, row_number) VALUES (7, 3, 9, 2);
INSERT INTO public.group_members (member_id, group_id, visitor_id, row_number) VALUES (8, 3, 10, 3);
INSERT INTO public.group_members (member_id, group_id, visitor_id, row_number) VALUES (9, 3, 11, 4);
INSERT INTO public.group_members (member_id, group_id, visitor_id, row_number) VALUES (10, 3, 12, 5);


--
-- TOC entry 5030 (class 0 OID 16496)
-- Dependencies: 229
-- Data for Name: group_requests; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.group_requests (group_id, request_id, template_file_path, photos_archive_path) VALUES (1, 3, '/templates/group_3.xlsx', '/photos/group_3/');
INSERT INTO public.group_requests (group_id, request_id, template_file_path, photos_archive_path) VALUES (2, 5, '/templates/group_5.xlsx', '/photos/group_5/');
INSERT INTO public.group_requests (group_id, request_id, template_file_path, photos_archive_path) VALUES (3, 7, NULL, NULL);


--
-- TOC entry 5028 (class 0 OID 16477)
-- Dependencies: 227
-- Data for Name: individual_requests; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.individual_requests (individual_id, request_id, visitor_id) VALUES (1, 1, 1);
INSERT INTO public.individual_requests (individual_id, request_id, visitor_id) VALUES (2, 2, 2);
INSERT INTO public.individual_requests (individual_id, request_id, visitor_id) VALUES (3, 4, 5);
INSERT INTO public.individual_requests (individual_id, request_id, visitor_id) VALUES (4, 6, 7);
INSERT INTO public.individual_requests (individual_id, request_id, visitor_id) VALUES (5, 8, 14);


--
-- TOC entry 5034 (class 0 OID 16529)
-- Dependencies: 233
-- Data for Name: passes; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.passes (pass_id, request_id, issued_at, issued_by, entry_time, exit_time) VALUES (1, 1, '2026-04-15 13:47:45.080524', 9404040, '2026-04-16 09:00:00', '2026-04-16 18:00:00');
INSERT INTO public.passes (pass_id, request_id, issued_at, issued_by, entry_time, exit_time) VALUES (2, 3, '2026-04-15 13:47:45.080524', 9404040, '2026-04-18 08:42:48.658867', '2026-04-18 08:42:51.089491');
INSERT INTO public.passes (pass_id, request_id, issued_at, issued_by, entry_time, exit_time) VALUES (3, 4, '2026-04-18 08:42:56.174247', 9404040, '2026-04-18 08:42:56.174247', NULL);
INSERT INTO public.passes (pass_id, request_id, issued_at, issued_by, entry_time, exit_time) VALUES (4, 8, '2026-04-19 18:57:38.369476', 9404040, '2026-04-19 18:57:38.369476', NULL);


--
-- TOC entry 5026 (class 0 OID 16444)
-- Dependencies: 225
-- Data for Name: requests; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.requests (request_id, user_id, type, start_date, end_date, purpose_id, department_id, employee_id, status, rejection_reason, comment, created_at) VALUES (1, 1, 'individual', '2026-04-16', '2026-04-18', 1, 1, 9367788, 'approved', NULL, 'Рабочая встреча по проекту', '2026-04-15 13:47:33.168273');
INSERT INTO public.requests (request_id, user_id, type, start_date, end_date, purpose_id, department_id, employee_id, status, rejection_reason, comment, created_at) VALUES (2, 2, 'individual', '2026-04-17', '2026-04-20', 2, 2, 9788737, 'pending', NULL, 'Экскурсия по производству', '2026-04-15 13:47:33.168273');
INSERT INTO public.requests (request_id, user_id, type, start_date, end_date, purpose_id, department_id, employee_id, status, rejection_reason, comment, created_at) VALUES (3, 3, 'group', '2026-04-16', '2026-04-16', 3, 1, 9367788, 'approved', NULL, 'Техническое обслуживание оборудования', '2026-04-15 13:47:33.168273');
INSERT INTO public.requests (request_id, user_id, type, start_date, end_date, purpose_id, department_id, employee_id, status, rejection_reason, comment, created_at) VALUES (4, 1, 'individual', '2026-04-18', '2026-04-22', 4, 5, 9737848, 'approved', NULL, 'Поставка оборудования - отклонено', '2026-04-15 13:47:33.168273');
INSERT INTO public.requests (request_id, user_id, type, start_date, end_date, purpose_id, department_id, employee_id, status, rejection_reason, comment, created_at) VALUES (7, 14, 'group', '2026-04-18', '2026-04-18', 2, 2, 9788737, 'approved', NULL, 'hfghdgfhdfgh', '2026-04-17 13:30:13.090786');
INSERT INTO public.requests (request_id, user_id, type, start_date, end_date, purpose_id, department_id, employee_id, status, rejection_reason, comment, created_at) VALUES (6, 6, 'individual', '2026-04-17', '2026-04-17', 2, 1, 9367788, 'approved', NULL, 'пвапавп', '2026-04-16 09:04:05.79587');
INSERT INTO public.requests (request_id, user_id, type, start_date, end_date, purpose_id, department_id, employee_id, status, rejection_reason, comment, created_at) VALUES (5, 4, 'group', '2026-04-20', '2026-04-25', 5, 3, 9736379, 'approved', NULL, 'Совещание с руководством', '2026-04-15 13:47:33.168273');
INSERT INTO public.requests (request_id, user_id, type, start_date, end_date, purpose_id, department_id, employee_id, status, rejection_reason, comment, created_at) VALUES (8, 1, 'individual', '2026-04-19', '2026-04-19', 1, 1, 9367788, 'approved', NULL, 'Тест', '2026-04-19 18:57:38.369476');


--
-- TOC entry 5017 (class 0 OID 16400)
-- Dependencies: 216
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.users (user_id, email, password_hash, registered_at, role) VALUES (1, 'user1@example.com', '6d5876c0d5c3b8c3b4d5c6e7f8a9b0c1', '2026-04-15 13:47:27.400728', 'user');
INSERT INTO public.users (user_id, email, password_hash, registered_at, role) VALUES (2, 'user2@example.com', '6d5876c0d5c3b8c3b4d5c6e7f8a9b0c1', '2026-04-15 13:47:27.400728', 'user');
INSERT INTO public.users (user_id, email, password_hash, registered_at, role) VALUES (3, 'user3@example.com', '6d5876c0d5c3b8c3b4d5c6e7f8a9b0c1', '2026-04-15 13:47:27.400728', 'user');
INSERT INTO public.users (user_id, email, password_hash, registered_at, role) VALUES (5, 'my@test.ru', 'a02ea1a2ad346192ddecbe1dc855b0e7', '2026-04-16 08:55:59.784925', 'user');
INSERT INTO public.users (user_id, email, password_hash, registered_at, role) VALUES (6, 'mytest@ru.com', 'a02ea1a2ad346192ddecbe1dc855b0e7', '2026-04-16 08:58:44.527101', 'user');
INSERT INTO public.users (user_id, email, password_hash, registered_at, role) VALUES (7, 'test@gmail.com', 'b2bb8cd3f82c95e28cc89ead6ea36616', '2026-04-16 09:05:53.868088', 'user');
INSERT INTO public.users (user_id, email, password_hash, registered_at, role) VALUES (4, 'admin@khranitel.ru', '2637a5c30af69a7bad877fdb65fbd78b', '2026-04-15 13:47:27.400728', 'admin');
INSERT INTO public.users (user_id, email, password_hash, registered_at, role) VALUES (8, 'put@gmail.com', '2168ad5e463d9accb215edaafa31c8d9', '2026-04-16 09:24:40.008201', 'user');
INSERT INTO public.users (user_id, email, password_hash, registered_at, role) VALUES (9, 'Test123@gmail.com', '2b923d7b39ca85a87fe6b3407a43b219', '2026-04-16 09:26:15.95073', 'user');
INSERT INTO public.users (user_id, email, password_hash, registered_at, role) VALUES (10, 'putnikov@gmail.com', '10487c8581423e8b2fbeed2b21c2cc53', '2026-04-16 09:27:04.870126', 'user');
INSERT INTO public.users (user_id, email, password_hash, registered_at, role) VALUES (11, 'test_proc@test.ru', '2168ad5e463d9accb215edaafa31c8d9', '2026-04-16 09:33:00.273832', 'user');
INSERT INTO public.users (user_id, email, password_hash, registered_at, role) VALUES (12, 'Test@gmail.ru', '10487c8581423e8b2fbeed2b21c2cc53', '2026-04-16 09:36:06.681928', 'user');
INSERT INTO public.users (user_id, email, password_hash, registered_at, role) VALUES (13, 'dimapytana@gmaail.com', '10487c8581423e8b2fbeed2b21c2cc53', '2026-04-17 13:21:24.295287', 'user');
INSERT INTO public.users (user_id, email, password_hash, registered_at, role) VALUES (14, 'dimapyt@ru.ru', '10487c8581423e8b2fbeed2b21c2cc53', '2026-04-17 13:22:35.174717', 'user');


--
-- TOC entry 5036 (class 0 OID 16549)
-- Dependencies: 235
-- Data for Name: visit_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.visit_logs (log_id, pass_id, recorded_by, recorded_at, comment) VALUES (1, 1, 9367788, '2026-04-15 13:47:47.887209', 'Посетитель прибыл вовремя');
INSERT INTO public.visit_logs (log_id, pass_id, recorded_by, recorded_at, comment) VALUES (2, 2, 9367788, '2026-04-15 13:47:47.887209', 'Группа на экскурсии');


--
-- TOC entry 5024 (class 0 OID 16437)
-- Dependencies: 223
-- Data for Name: visit_purposes; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.visit_purposes (purpose_id, name) VALUES (1, 'Рабочая встреча');
INSERT INTO public.visit_purposes (purpose_id, name) VALUES (2, 'Экскурсия');
INSERT INTO public.visit_purposes (purpose_id, name) VALUES (3, 'Техническое обслуживание');
INSERT INTO public.visit_purposes (purpose_id, name) VALUES (4, 'Поставка оборудования');
INSERT INTO public.visit_purposes (purpose_id, name) VALUES (5, 'Совещание');


--
-- TOC entry 5019 (class 0 OID 16411)
-- Dependencies: 218
-- Data for Name: visitors; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) VALUES (1, 'Степанова', 'Радинка', 'Власовна', '+7 (613) 272-60-62', 'Radinka100@yandex.ru', 'ООО Ромашка', '1986-10-18', '0208', '530509', NULL, '/scans/stepanova.pdf', NULL);
INSERT INTO public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) VALUES (2, 'Шилов', 'Прохор', 'Герасимович', '+7 (615) 594-77-66', 'Prohor156@list.ru', 'ЗАО Берёзка', '1977-10-09', '3036', '796488', NULL, '/scans/shilov.pdf', NULL);
INSERT INTO public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) VALUES (3, 'Кононов', 'Юрин', 'Романович', '+7 (784) 673-51-91', 'YUrin155@gmail.com', 'ИП Кононов', '1971-10-08', '2747', '790512', NULL, '/scans/kononov.pdf', NULL);
INSERT INTO public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) VALUES (4, 'Иванов', 'Сергей', 'Петрович', '+7 (916) 123-45-67', 'ivanov@mail.ru', 'ООО Ромашка', '1986-10-16', '2219', '123456', NULL, '/scans/ivanov.pdf', NULL);
INSERT INTO public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) VALUES (5, 'Петрова', 'Анна', 'Сергеевна', '+7 (915) 234-56-78', 'petrova@mail.ru', 'ЗАО Берёзка', '1990-05-20', '3320', '234567', NULL, '/scans/petrova.pdf', NULL);
INSERT INTO public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) VALUES (6, 'Сидоров', 'Иван', 'Алексеевич', '+7 (917) 345-67-89', 'sidorov@mail.ru', 'ИП Сидоров', '1988-12-01', '4431', '345678', NULL, '/scans/sidorov.pdf', NULL);
INSERT INTO public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) VALUES (7, 'Путников ', 'Дмитрий', 'Васильевич', '+7 9018006384', 'dimapytana@gmail.com', 'ООО СЛОБКОЛЛ', '2000-04-15', '2232', '153242', NULL, 'C:\Users\User\Downloads\Лабораторная работа (1).pdf', NULL);
INSERT INTO public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) VALUES (8, 'впапва', 'пвапва', 'павпв', '+7 42124214', 'впывпвп@gdsgds', NULL, '2000-04-10', '3423', '421424', NULL, '', NULL);
INSERT INTO public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) VALUES (9, 'hfdhfdshfdh', 'sfdhfdshsdfh', 'sfdhsdfhfdsh', '+7 5325325', 'dsafgsgdsg@hdfhfd', NULL, '2000-04-10', '4224', '251255', NULL, '', NULL);
INSERT INTO public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) VALUES (10, 'dfjgfdgjndgfndfn', 'gfhsgfhdgfh', 'gfshjsgfhsgfhsdfh', '+7 ', 'agsdgdsgadsg@', NULL, '2000-04-10', '2424', '343243', NULL, '', NULL);
INSERT INTO public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) VALUES (11, 'hdfshgfndndgfn', 'hfghfgjdgjgjdfjgfd', 'fgjdfjdgfjdgfjdf', '+7 ', 'fdshfjsgfjgfsj@', NULL, '2000-04-10', '5353', '532532', NULL, '', NULL);
INSERT INTO public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) VALUES (12, 'gdjdgfhdhdbgfbhdgfhbd', 'dfgbhdfgbhdfghbdgf', 'hbdfgbhdgfbhdgf', '+7 ', 'hfdshbfhdgfnbhgmn@', NULL, '2000-04-10', '5353', '352523', NULL, '', NULL);
INSERT INTO public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) VALUES (13, 'Тестов', 'Тест', NULL, NULL, 'testuser@example.com', NULL, '1990-01-01', '1234', '567890', NULL, '/scans/test.pdf', 'testuser');
INSERT INTO public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path, login) VALUES (14, 'Тестов', 'Тест', NULL, NULL, 'test@test.ru', NULL, '1990-01-01', '1111', '111111', NULL, '/scans/test.pdf', 'test');


--
-- TOC entry 5056 (class 0 OID 0)
-- Dependencies: 237
-- Name: blacklist_blacklist_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.blacklist_blacklist_id_seq', 1, true);


--
-- TOC entry 5057 (class 0 OID 0)
-- Dependencies: 219
-- Name: departments_department_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.departments_department_id_seq', 5, true);


--
-- TOC entry 5058 (class 0 OID 0)
-- Dependencies: 230
-- Name: group_members_member_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.group_members_member_id_seq', 10, true);


--
-- TOC entry 5059 (class 0 OID 0)
-- Dependencies: 228
-- Name: group_requests_group_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.group_requests_group_id_seq', 3, true);


--
-- TOC entry 5060 (class 0 OID 0)
-- Dependencies: 226
-- Name: individual_requests_individual_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.individual_requests_individual_id_seq', 5, true);


--
-- TOC entry 5061 (class 0 OID 0)
-- Dependencies: 232
-- Name: passes_pass_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.passes_pass_id_seq', 4, true);


--
-- TOC entry 5062 (class 0 OID 0)
-- Dependencies: 224
-- Name: requests_request_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.requests_request_id_seq', 8, true);


--
-- TOC entry 5063 (class 0 OID 0)
-- Dependencies: 215
-- Name: users_user_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.users_user_id_seq', 14, true);


--
-- TOC entry 5064 (class 0 OID 0)
-- Dependencies: 234
-- Name: visit_logs_log_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.visit_logs_log_id_seq', 2, true);


--
-- TOC entry 5065 (class 0 OID 0)
-- Dependencies: 222
-- Name: visit_purposes_purpose_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.visit_purposes_purpose_id_seq', 5, true);


--
-- TOC entry 5066 (class 0 OID 0)
-- Dependencies: 217
-- Name: visitors_visitor_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.visitors_visitor_id_seq', 14, true);


--
-- TOC entry 4854 (class 2606 OID 16657)
-- Name: blacklist blacklist_passport_number_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.blacklist
    ADD CONSTRAINT blacklist_passport_number_key UNIQUE (passport_number);


--
-- TOC entry 4856 (class 2606 OID 16655)
-- Name: blacklist blacklist_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.blacklist
    ADD CONSTRAINT blacklist_pkey PRIMARY KEY (blacklist_id);


--
-- TOC entry 4828 (class 2606 OID 16425)
-- Name: departments departments_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.departments
    ADD CONSTRAINT departments_pkey PRIMARY KEY (department_id);


--
-- TOC entry 4830 (class 2606 OID 16430)
-- Name: employees employees_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employees
    ADD CONSTRAINT employees_pkey PRIMARY KEY (employee_id);


--
-- TOC entry 4846 (class 2606 OID 16517)
-- Name: group_members group_members_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_members
    ADD CONSTRAINT group_members_pkey PRIMARY KEY (member_id);


--
-- TOC entry 4842 (class 2606 OID 16503)
-- Name: group_requests group_requests_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_requests
    ADD CONSTRAINT group_requests_pkey PRIMARY KEY (group_id);


--
-- TOC entry 4844 (class 2606 OID 16505)
-- Name: group_requests group_requests_request_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_requests
    ADD CONSTRAINT group_requests_request_id_key UNIQUE (request_id);


--
-- TOC entry 4838 (class 2606 OID 16482)
-- Name: individual_requests individual_requests_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.individual_requests
    ADD CONSTRAINT individual_requests_pkey PRIMARY KEY (individual_id);


--
-- TOC entry 4840 (class 2606 OID 16484)
-- Name: individual_requests individual_requests_request_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.individual_requests
    ADD CONSTRAINT individual_requests_request_id_key UNIQUE (request_id);


--
-- TOC entry 4848 (class 2606 OID 16535)
-- Name: passes passes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.passes
    ADD CONSTRAINT passes_pkey PRIMARY KEY (pass_id);


--
-- TOC entry 4850 (class 2606 OID 16537)
-- Name: passes passes_request_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.passes
    ADD CONSTRAINT passes_request_id_key UNIQUE (request_id);


--
-- TOC entry 4836 (class 2606 OID 16455)
-- Name: requests requests_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_pkey PRIMARY KEY (request_id);


--
-- TOC entry 4819 (class 2606 OID 16409)
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- TOC entry 4821 (class 2606 OID 16407)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (user_id);


--
-- TOC entry 4852 (class 2606 OID 16557)
-- Name: visit_logs visit_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_logs
    ADD CONSTRAINT visit_logs_pkey PRIMARY KEY (log_id);


--
-- TOC entry 4832 (class 2606 OID 16442)
-- Name: visit_purposes visit_purposes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_purposes
    ADD CONSTRAINT visit_purposes_pkey PRIMARY KEY (purpose_id);


--
-- TOC entry 4824 (class 2606 OID 16640)
-- Name: visitors visitors_login_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visitors
    ADD CONSTRAINT visitors_login_key UNIQUE (login);


--
-- TOC entry 4826 (class 2606 OID 16418)
-- Name: visitors visitors_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visitors
    ADD CONSTRAINT visitors_pkey PRIMARY KEY (visitor_id);


--
-- TOC entry 4833 (class 1259 OID 16569)
-- Name: idx_requests_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_requests_status ON public.requests USING btree (status);


--
-- TOC entry 4834 (class 1259 OID 16568)
-- Name: idx_requests_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_requests_user_id ON public.requests USING btree (user_id);


--
-- TOC entry 4822 (class 1259 OID 16570)
-- Name: idx_visitors_passport; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_visitors_passport ON public.visitors USING btree (passport_series, passport_number);


--
-- TOC entry 4871 (class 2620 OID 16644)
-- Name: visitors trg_visitor_login; Type: TRIGGER; Schema: public; Owner: postgres
--

CREATE TRIGGER trg_visitor_login BEFORE INSERT ON public.visitors FOR EACH ROW EXECUTE FUNCTION public.generate_visitor_login();


--
-- TOC entry 4857 (class 2606 OID 16431)
-- Name: employees employees_department_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employees
    ADD CONSTRAINT employees_department_id_fkey FOREIGN KEY (department_id) REFERENCES public.departments(department_id);


--
-- TOC entry 4865 (class 2606 OID 16518)
-- Name: group_members group_members_group_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_members
    ADD CONSTRAINT group_members_group_id_fkey FOREIGN KEY (group_id) REFERENCES public.group_requests(group_id) ON DELETE CASCADE;


--
-- TOC entry 4866 (class 2606 OID 16523)
-- Name: group_members group_members_visitor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_members
    ADD CONSTRAINT group_members_visitor_id_fkey FOREIGN KEY (visitor_id) REFERENCES public.visitors(visitor_id);


--
-- TOC entry 4864 (class 2606 OID 16506)
-- Name: group_requests group_requests_request_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_requests
    ADD CONSTRAINT group_requests_request_id_fkey FOREIGN KEY (request_id) REFERENCES public.requests(request_id) ON DELETE CASCADE;


--
-- TOC entry 4862 (class 2606 OID 16485)
-- Name: individual_requests individual_requests_request_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.individual_requests
    ADD CONSTRAINT individual_requests_request_id_fkey FOREIGN KEY (request_id) REFERENCES public.requests(request_id) ON DELETE CASCADE;


--
-- TOC entry 4863 (class 2606 OID 16490)
-- Name: individual_requests individual_requests_visitor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.individual_requests
    ADD CONSTRAINT individual_requests_visitor_id_fkey FOREIGN KEY (visitor_id) REFERENCES public.visitors(visitor_id);


--
-- TOC entry 4867 (class 2606 OID 16543)
-- Name: passes passes_issued_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.passes
    ADD CONSTRAINT passes_issued_by_fkey FOREIGN KEY (issued_by) REFERENCES public.employees(employee_id);


--
-- TOC entry 4868 (class 2606 OID 16538)
-- Name: passes passes_request_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.passes
    ADD CONSTRAINT passes_request_id_fkey FOREIGN KEY (request_id) REFERENCES public.requests(request_id) ON DELETE CASCADE;


--
-- TOC entry 4858 (class 2606 OID 16466)
-- Name: requests requests_department_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_department_id_fkey FOREIGN KEY (department_id) REFERENCES public.departments(department_id);


--
-- TOC entry 4859 (class 2606 OID 16471)
-- Name: requests requests_employee_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_employee_id_fkey FOREIGN KEY (employee_id) REFERENCES public.employees(employee_id);


--
-- TOC entry 4860 (class 2606 OID 16461)
-- Name: requests requests_purpose_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_purpose_id_fkey FOREIGN KEY (purpose_id) REFERENCES public.visit_purposes(purpose_id);


--
-- TOC entry 4861 (class 2606 OID 16456)
-- Name: requests requests_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(user_id);


--
-- TOC entry 4869 (class 2606 OID 16558)
-- Name: visit_logs visit_logs_pass_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_logs
    ADD CONSTRAINT visit_logs_pass_id_fkey FOREIGN KEY (pass_id) REFERENCES public.passes(pass_id) ON DELETE CASCADE;


--
-- TOC entry 4870 (class 2606 OID 16563)
-- Name: visit_logs visit_logs_recorded_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_logs
    ADD CONSTRAINT visit_logs_recorded_by_fkey FOREIGN KEY (recorded_by) REFERENCES public.employees(employee_id);


-- Completed on 2026-04-19 19:38:22

--
-- PostgreSQL database dump complete
--

\unrestrict eYxZLsdowPvicZxqwfALPGb3RX7jugXYPKQ347ataXdNVC0hgsrljmhPjY8W6dg

