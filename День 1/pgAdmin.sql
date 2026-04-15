--
-- PostgreSQL database dump
--

\restrict 1CVQPWszpnfmytoRQf2ViWQ0u0bxWXVGbWsLdTiPrflY3mXIoAwmJZYWg03lx6w

-- Dumped from database version 16.13
-- Dumped by pg_dump version 18.1

-- Started on 2026-04-15 14:08:51

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
-- TOC entry 5018 (class 0 OID 0)
-- Dependencies: 4
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: pg_database_owner
--

COMMENT ON SCHEMA public IS 'standard public schema';


SET default_tablespace = '';

SET default_table_access_method = heap;

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
-- TOC entry 5019 (class 0 OID 0)
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
-- TOC entry 5020 (class 0 OID 0)
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
-- TOC entry 5021 (class 0 OID 0)
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
-- TOC entry 5022 (class 0 OID 0)
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
-- TOC entry 5023 (class 0 OID 0)
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
-- TOC entry 5024 (class 0 OID 0)
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
-- TOC entry 5025 (class 0 OID 0)
-- Dependencies: 215
-- Name: users_user_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_user_id_seq OWNED BY public.users.user_id;


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
-- TOC entry 5026 (class 0 OID 0)
-- Dependencies: 234
-- Name: visit_logs_log_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.visit_logs_log_id_seq OWNED BY public.visit_logs.log_id;


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
-- TOC entry 5027 (class 0 OID 0)
-- Dependencies: 222
-- Name: visit_purposes_purpose_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.visit_purposes_purpose_id_seq OWNED BY public.visit_purposes.purpose_id;


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
    passport_scan_path character varying(500) NOT NULL
);


ALTER TABLE public.visitors OWNER TO postgres;

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
-- TOC entry 5028 (class 0 OID 0)
-- Dependencies: 217
-- Name: visitors_visitor_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.visitors_visitor_id_seq OWNED BY public.visitors.visitor_id;


--
-- TOC entry 4788 (class 2604 OID 16423)
-- Name: departments department_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.departments ALTER COLUMN department_id SET DEFAULT nextval('public.departments_department_id_seq'::regclass);


--
-- TOC entry 4795 (class 2604 OID 16515)
-- Name: group_members member_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_members ALTER COLUMN member_id SET DEFAULT nextval('public.group_members_member_id_seq'::regclass);


--
-- TOC entry 4794 (class 2604 OID 16499)
-- Name: group_requests group_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_requests ALTER COLUMN group_id SET DEFAULT nextval('public.group_requests_group_id_seq'::regclass);


--
-- TOC entry 4793 (class 2604 OID 16480)
-- Name: individual_requests individual_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.individual_requests ALTER COLUMN individual_id SET DEFAULT nextval('public.individual_requests_individual_id_seq'::regclass);


--
-- TOC entry 4796 (class 2604 OID 16532)
-- Name: passes pass_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.passes ALTER COLUMN pass_id SET DEFAULT nextval('public.passes_pass_id_seq'::regclass);


--
-- TOC entry 4790 (class 2604 OID 16447)
-- Name: requests request_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests ALTER COLUMN request_id SET DEFAULT nextval('public.requests_request_id_seq'::regclass);


--
-- TOC entry 4784 (class 2604 OID 16403)
-- Name: users user_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN user_id SET DEFAULT nextval('public.users_user_id_seq'::regclass);


--
-- TOC entry 4798 (class 2604 OID 16552)
-- Name: visit_logs log_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_logs ALTER COLUMN log_id SET DEFAULT nextval('public.visit_logs_log_id_seq'::regclass);


--
-- TOC entry 4789 (class 2604 OID 16440)
-- Name: visit_purposes purpose_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_purposes ALTER COLUMN purpose_id SET DEFAULT nextval('public.visit_purposes_purpose_id_seq'::regclass);


--
-- TOC entry 4787 (class 2604 OID 16414)
-- Name: visitors visitor_id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visitors ALTER COLUMN visitor_id SET DEFAULT nextval('public.visitors_visitor_id_seq'::regclass);


--
-- TOC entry 4997 (class 0 OID 16420)
-- Dependencies: 220
-- Data for Name: departments; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.departments (department_id, name) FROM stdin;
1	Производство
2	Сбыт
3	Администрация
4	Служба безопасности
5	Планирование
\.


--
-- TOC entry 4998 (class 0 OID 16426)
-- Dependencies: 221
-- Data for Name: employees; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.employees (employee_id, full_name, department_id, section) FROM stdin;
9367788	Фомичева Авдотья Трофимовна	1	\N
9788737	Гаврилова Римма Ефимовна	2	\N
9736379	Носкова Наталия Прохоровна	3	\N
9362832	Архипов Тимофей Васильевич	4	\N
9737848	Орехова Вероника Артемовна	5	\N
9768239	Савельев Павел Степанович	\N	Общий отдел
9404040	Чернов Всеволод Наумович	\N	Охрана
\.


--
-- TOC entry 5008 (class 0 OID 16512)
-- Dependencies: 231
-- Data for Name: group_members; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.group_members (member_id, group_id, visitor_id, row_number) FROM stdin;
1	1	1	1
2	1	2	2
3	1	3	3
4	1	4	4
5	1	5	5
\.


--
-- TOC entry 5006 (class 0 OID 16496)
-- Dependencies: 229
-- Data for Name: group_requests; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.group_requests (group_id, request_id, template_file_path, photos_archive_path) FROM stdin;
1	3	/templates/group_3.xlsx	/photos/group_3/
2	5	/templates/group_5.xlsx	/photos/group_5/
\.


--
-- TOC entry 5004 (class 0 OID 16477)
-- Dependencies: 227
-- Data for Name: individual_requests; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.individual_requests (individual_id, request_id, visitor_id) FROM stdin;
1	1	1
2	2	2
3	4	5
\.


--
-- TOC entry 5010 (class 0 OID 16529)
-- Dependencies: 233
-- Data for Name: passes; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.passes (pass_id, request_id, issued_at, issued_by, entry_time, exit_time) FROM stdin;
1	1	2026-04-15 13:47:45.080524	9404040	2026-04-16 09:00:00	2026-04-16 18:00:00
2	3	2026-04-15 13:47:45.080524	9404040	2026-04-16 10:00:00	\N
\.


--
-- TOC entry 5002 (class 0 OID 16444)
-- Dependencies: 225
-- Data for Name: requests; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.requests (request_id, user_id, type, start_date, end_date, purpose_id, department_id, employee_id, status, rejection_reason, comment, created_at) FROM stdin;
1	1	individual	2026-04-16	2026-04-18	1	1	9367788	approved	\N	Рабочая встреча по проекту	2026-04-15 13:47:33.168273
2	2	individual	2026-04-17	2026-04-20	2	2	9788737	pending	\N	Экскурсия по производству	2026-04-15 13:47:33.168273
3	3	group	2026-04-16	2026-04-16	3	1	9367788	approved	\N	Техническое обслуживание оборудования	2026-04-15 13:47:33.168273
4	1	individual	2026-04-18	2026-04-22	4	5	9737848	rejected	\N	Поставка оборудования - отклонено	2026-04-15 13:47:33.168273
5	4	group	2026-04-20	2026-04-25	5	3	9736379	pending	\N	Совещание с руководством	2026-04-15 13:47:33.168273
\.


--
-- TOC entry 4993 (class 0 OID 16400)
-- Dependencies: 216
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (user_id, email, password_hash, registered_at, role) FROM stdin;
1	user1@example.com	6d5876c0d5c3b8c3b4d5c6e7f8a9b0c1	2026-04-15 13:47:27.400728	user
2	user2@example.com	6d5876c0d5c3b8c3b4d5c6e7f8a9b0c1	2026-04-15 13:47:27.400728	user
3	user3@example.com	6d5876c0d5c3b8c3b4d5c6e7f8a9b0c1	2026-04-15 13:47:27.400728	user
4	admin@khranitel.ru	6d5876c0d5c3b8c3b4d5c6e7f8a9b0c1	2026-04-15 13:47:27.400728	admin
\.


--
-- TOC entry 5012 (class 0 OID 16549)
-- Dependencies: 235
-- Data for Name: visit_logs; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.visit_logs (log_id, pass_id, recorded_by, recorded_at, comment) FROM stdin;
1	1	9367788	2026-04-15 13:47:47.887209	Посетитель прибыл вовремя
2	2	9367788	2026-04-15 13:47:47.887209	Группа на экскурсии
\.


--
-- TOC entry 5000 (class 0 OID 16437)
-- Dependencies: 223
-- Data for Name: visit_purposes; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.visit_purposes (purpose_id, name) FROM stdin;
1	Рабочая встреча
2	Экскурсия
3	Техническое обслуживание
4	Поставка оборудования
5	Совещание
\.


--
-- TOC entry 4995 (class 0 OID 16411)
-- Dependencies: 218
-- Data for Name: visitors; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.visitors (visitor_id, last_name, first_name, middle_name, phone, email, organization, birth_date, passport_series, passport_number, photo_path, passport_scan_path) FROM stdin;
1	Степанова	Радинка	Власовна	+7 (613) 272-60-62	Radinka100@yandex.ru	ООО Ромашка	1986-10-18	0208	530509	\N	/scans/stepanova.pdf
2	Шилов	Прохор	Герасимович	+7 (615) 594-77-66	Prohor156@list.ru	ЗАО Берёзка	1977-10-09	3036	796488	\N	/scans/shilov.pdf
3	Кононов	Юрин	Романович	+7 (784) 673-51-91	YUrin155@gmail.com	ИП Кононов	1971-10-08	2747	790512	\N	/scans/kononov.pdf
4	Иванов	Сергей	Петрович	+7 (916) 123-45-67	ivanov@mail.ru	ООО Ромашка	1986-10-16	2219	123456	\N	/scans/ivanov.pdf
5	Петрова	Анна	Сергеевна	+7 (915) 234-56-78	petrova@mail.ru	ЗАО Берёзка	1990-05-20	3320	234567	\N	/scans/petrova.pdf
6	Сидоров	Иван	Алексеевич	+7 (917) 345-67-89	sidorov@mail.ru	ИП Сидоров	1988-12-01	4431	345678	\N	/scans/sidorov.pdf
\.


--
-- TOC entry 5029 (class 0 OID 0)
-- Dependencies: 219
-- Name: departments_department_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.departments_department_id_seq', 5, true);


--
-- TOC entry 5030 (class 0 OID 0)
-- Dependencies: 230
-- Name: group_members_member_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.group_members_member_id_seq', 5, true);


--
-- TOC entry 5031 (class 0 OID 0)
-- Dependencies: 228
-- Name: group_requests_group_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.group_requests_group_id_seq', 2, true);


--
-- TOC entry 5032 (class 0 OID 0)
-- Dependencies: 226
-- Name: individual_requests_individual_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.individual_requests_individual_id_seq', 3, true);


--
-- TOC entry 5033 (class 0 OID 0)
-- Dependencies: 232
-- Name: passes_pass_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.passes_pass_id_seq', 2, true);


--
-- TOC entry 5034 (class 0 OID 0)
-- Dependencies: 224
-- Name: requests_request_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.requests_request_id_seq', 5, true);


--
-- TOC entry 5035 (class 0 OID 0)
-- Dependencies: 215
-- Name: users_user_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.users_user_id_seq', 4, true);


--
-- TOC entry 5036 (class 0 OID 0)
-- Dependencies: 234
-- Name: visit_logs_log_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.visit_logs_log_id_seq', 2, true);


--
-- TOC entry 5037 (class 0 OID 0)
-- Dependencies: 222
-- Name: visit_purposes_purpose_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.visit_purposes_purpose_id_seq', 5, true);


--
-- TOC entry 5038 (class 0 OID 0)
-- Dependencies: 217
-- Name: visitors_visitor_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.visitors_visitor_id_seq', 6, true);


--
-- TOC entry 4810 (class 2606 OID 16425)
-- Name: departments departments_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.departments
    ADD CONSTRAINT departments_pkey PRIMARY KEY (department_id);


--
-- TOC entry 4812 (class 2606 OID 16430)
-- Name: employees employees_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employees
    ADD CONSTRAINT employees_pkey PRIMARY KEY (employee_id);


--
-- TOC entry 4828 (class 2606 OID 16517)
-- Name: group_members group_members_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_members
    ADD CONSTRAINT group_members_pkey PRIMARY KEY (member_id);


--
-- TOC entry 4824 (class 2606 OID 16503)
-- Name: group_requests group_requests_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_requests
    ADD CONSTRAINT group_requests_pkey PRIMARY KEY (group_id);


--
-- TOC entry 4826 (class 2606 OID 16505)
-- Name: group_requests group_requests_request_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_requests
    ADD CONSTRAINT group_requests_request_id_key UNIQUE (request_id);


--
-- TOC entry 4820 (class 2606 OID 16482)
-- Name: individual_requests individual_requests_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.individual_requests
    ADD CONSTRAINT individual_requests_pkey PRIMARY KEY (individual_id);


--
-- TOC entry 4822 (class 2606 OID 16484)
-- Name: individual_requests individual_requests_request_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.individual_requests
    ADD CONSTRAINT individual_requests_request_id_key UNIQUE (request_id);


--
-- TOC entry 4830 (class 2606 OID 16535)
-- Name: passes passes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.passes
    ADD CONSTRAINT passes_pkey PRIMARY KEY (pass_id);


--
-- TOC entry 4832 (class 2606 OID 16537)
-- Name: passes passes_request_id_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.passes
    ADD CONSTRAINT passes_request_id_key UNIQUE (request_id);


--
-- TOC entry 4818 (class 2606 OID 16455)
-- Name: requests requests_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_pkey PRIMARY KEY (request_id);


--
-- TOC entry 4803 (class 2606 OID 16409)
-- Name: users users_email_key; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_email_key UNIQUE (email);


--
-- TOC entry 4805 (class 2606 OID 16407)
-- Name: users users_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pkey PRIMARY KEY (user_id);


--
-- TOC entry 4834 (class 2606 OID 16557)
-- Name: visit_logs visit_logs_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_logs
    ADD CONSTRAINT visit_logs_pkey PRIMARY KEY (log_id);


--
-- TOC entry 4814 (class 2606 OID 16442)
-- Name: visit_purposes visit_purposes_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_purposes
    ADD CONSTRAINT visit_purposes_pkey PRIMARY KEY (purpose_id);


--
-- TOC entry 4808 (class 2606 OID 16418)
-- Name: visitors visitors_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visitors
    ADD CONSTRAINT visitors_pkey PRIMARY KEY (visitor_id);


--
-- TOC entry 4815 (class 1259 OID 16569)
-- Name: idx_requests_status; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_requests_status ON public.requests USING btree (status);


--
-- TOC entry 4816 (class 1259 OID 16568)
-- Name: idx_requests_user_id; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_requests_user_id ON public.requests USING btree (user_id);


--
-- TOC entry 4806 (class 1259 OID 16570)
-- Name: idx_visitors_passport; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX idx_visitors_passport ON public.visitors USING btree (passport_series, passport_number);


--
-- TOC entry 4835 (class 2606 OID 16431)
-- Name: employees employees_department_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.employees
    ADD CONSTRAINT employees_department_id_fkey FOREIGN KEY (department_id) REFERENCES public.departments(department_id);


--
-- TOC entry 4843 (class 2606 OID 16518)
-- Name: group_members group_members_group_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_members
    ADD CONSTRAINT group_members_group_id_fkey FOREIGN KEY (group_id) REFERENCES public.group_requests(group_id) ON DELETE CASCADE;


--
-- TOC entry 4844 (class 2606 OID 16523)
-- Name: group_members group_members_visitor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_members
    ADD CONSTRAINT group_members_visitor_id_fkey FOREIGN KEY (visitor_id) REFERENCES public.visitors(visitor_id);


--
-- TOC entry 4842 (class 2606 OID 16506)
-- Name: group_requests group_requests_request_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.group_requests
    ADD CONSTRAINT group_requests_request_id_fkey FOREIGN KEY (request_id) REFERENCES public.requests(request_id) ON DELETE CASCADE;


--
-- TOC entry 4840 (class 2606 OID 16485)
-- Name: individual_requests individual_requests_request_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.individual_requests
    ADD CONSTRAINT individual_requests_request_id_fkey FOREIGN KEY (request_id) REFERENCES public.requests(request_id) ON DELETE CASCADE;


--
-- TOC entry 4841 (class 2606 OID 16490)
-- Name: individual_requests individual_requests_visitor_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.individual_requests
    ADD CONSTRAINT individual_requests_visitor_id_fkey FOREIGN KEY (visitor_id) REFERENCES public.visitors(visitor_id);


--
-- TOC entry 4845 (class 2606 OID 16543)
-- Name: passes passes_issued_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.passes
    ADD CONSTRAINT passes_issued_by_fkey FOREIGN KEY (issued_by) REFERENCES public.employees(employee_id);


--
-- TOC entry 4846 (class 2606 OID 16538)
-- Name: passes passes_request_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.passes
    ADD CONSTRAINT passes_request_id_fkey FOREIGN KEY (request_id) REFERENCES public.requests(request_id) ON DELETE CASCADE;


--
-- TOC entry 4836 (class 2606 OID 16466)
-- Name: requests requests_department_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_department_id_fkey FOREIGN KEY (department_id) REFERENCES public.departments(department_id);


--
-- TOC entry 4837 (class 2606 OID 16471)
-- Name: requests requests_employee_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_employee_id_fkey FOREIGN KEY (employee_id) REFERENCES public.employees(employee_id);


--
-- TOC entry 4838 (class 2606 OID 16461)
-- Name: requests requests_purpose_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_purpose_id_fkey FOREIGN KEY (purpose_id) REFERENCES public.visit_purposes(purpose_id);


--
-- TOC entry 4839 (class 2606 OID 16456)
-- Name: requests requests_user_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.requests
    ADD CONSTRAINT requests_user_id_fkey FOREIGN KEY (user_id) REFERENCES public.users(user_id);


--
-- TOC entry 4847 (class 2606 OID 16558)
-- Name: visit_logs visit_logs_pass_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_logs
    ADD CONSTRAINT visit_logs_pass_id_fkey FOREIGN KEY (pass_id) REFERENCES public.passes(pass_id) ON DELETE CASCADE;


--
-- TOC entry 4848 (class 2606 OID 16563)
-- Name: visit_logs visit_logs_recorded_by_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.visit_logs
    ADD CONSTRAINT visit_logs_recorded_by_fkey FOREIGN KEY (recorded_by) REFERENCES public.employees(employee_id);


-- Completed on 2026-04-15 14:08:51

--
-- PostgreSQL database dump complete
--

\unrestrict 1CVQPWszpnfmytoRQf2ViWQ0u0bxWXVGbWsLdTiPrflY3mXIoAwmJZYWg03lx6w

