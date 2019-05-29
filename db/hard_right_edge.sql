--
-- PostgreSQL database dump
--

-- NB! Run from command prompt: psql -U postgres -f hard_right_edge.sql

SET statement_timeout = 0;
SET lock_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

DO 
$do$
BEGIN
  IF NOT EXISTS (SELECT                      
                FROM   pg_catalog.pg_roles
                WHERE  rolname = 'hard_right_edge_app') THEN
    CREATE ROLE hard_right_edge_app LOGIN ENCRYPTED PASSWORD 'md5cd3df2fc5f80cc9f8881b442fe3582ee'
      CREATEDB
      VALID UNTIL 'infinity';
  END IF;
END;
$do$;

DROP DATABASE hard_right_edge;
--
-- TOC entry 2130 (class 1262 OID 74315)
-- Name: hard_right_edge; Type: DATABASE; Schema: -; Owner: hard_right_edge_app
--

CREATE DATABASE hard_right_edge WITH TEMPLATE = template0 ENCODING = 'UTF8' LC_COLLATE = 'English_South Africa.1252' LC_CTYPE = 'English_South Africa.1252';


ALTER DATABASE hard_right_edge OWNER TO hard_right_edge_app;

\connect hard_right_edge

SET statement_timeout = 0;
SET lock_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 6 (class 2615 OID 2200)
-- Name: public; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA public;
DO 
$do$
BEGIN
  IF NOT EXISTS (SELECT schema_name 
                FROM    information_schema.schemata 
                WHERE   schema_name = 'public') THEN
    ALTER SCHEMA public OWNER TO postgres;
  END IF;
END;
$do$;

--
-- TOC entry 2131 (class 0 OID 0)
-- Dependencies: 6
-- Name: SCHEMA public; Type: COMMENT; Schema: -; Owner: postgres
--

COMMENT ON SCHEMA public IS 'standard public schema';


--
-- TOC entry 1 (class 3079 OID 12355)
-- Name: plpgsql; Type: EXTENSION; Schema: -; Owner: 
--

CREATE EXTENSION IF NOT EXISTS plpgsql WITH SCHEMA pg_catalog;


--
-- TOC entry 2133 (class 0 OID 0)
-- Dependencies: 1
-- Name: EXTENSION plpgsql; Type: COMMENT; Schema: -; Owner: 
--

COMMENT ON EXTENSION plpgsql IS 'PL/pgSQL procedural language';


SET search_path = public, pg_catalog;

SET default_tablespace = '';

SET default_with_oids = false;

--
-- TOC entry 184 (class 1259 OID 74332)
-- Name: security; Type: TABLE; Schema: public; Owner: hard_right_edge_app
--

CREATE TABLE security (
    id bigint NOT NULL,
    name character varying(150) NOT NULL,
    previous_name character varying(150)
);


ALTER TABLE security OWNER TO hard_right_edge_app;

--
-- TOC entry 183 (class 1259 OID 74330)
-- Name: security_id_seq; Type: SEQUENCE; Schema: public; Owner: hard_right_edge_app
--

CREATE SEQUENCE security_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE security_id_seq OWNER TO hard_right_edge_app;

--
-- TOC entry 2134 (class 0 OID 0)
-- Dependencies: 183
-- Name: security_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: hard_right_edge_app
--

ALTER SEQUENCE security_id_seq OWNED BY security.id;


--
-- TOC entry 185 (class 1259 OID 74338)
-- Name: security_platform; Type: TABLE; Schema: public; Owner: hard_right_edge_app
--

CREATE TABLE security_platform (
    security_id bigint NOT NULL,
    platform_id integer NOT NULL,
    symbol character varying(150) NOT NULL
);


ALTER TABLE security_platform OWNER TO hard_right_edge_app;

--
-- TOC entry 187 (class 1259 OID 74373)
-- Name: security_price; Type: TABLE; Schema: public; Owner: hard_right_edge_app
--

CREATE TABLE security_price (
    id bigint NOT NULL,
    security_id bigint NOT NULL,
    date date NOT NULL,
    openp double precision NOT NULL,
    high double precision NOT NULL,
    low double precision NOT NULL,
    close double precision NOT NULL,
    adj_close double precision,
    volume bigint NOT NULL
);


ALTER TABLE security_price OWNER TO hard_right_edge_app;

--
-- TOC entry 186 (class 1259 OID 74371)
-- Name: security_price_id_seq; Type: SEQUENCE; Schema: public; Owner: hard_right_edge_app
--

CREATE SEQUENCE security_price_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE security_price_id_seq OWNER TO hard_right_edge_app;

--
-- TOC entry 2135 (class 0 OID 0)
-- Dependencies: 186
-- Name: security_price_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: hard_right_edge_app
--

ALTER SEQUENCE security_price_id_seq OWNED BY security_price.id;


--
-- TOC entry 182 (class 1259 OID 74318)
-- Name: transaction; Type: TABLE; Schema: public; Owner: hard_right_edge_app
--

CREATE TABLE transaction (
    id integer NOT NULL,
    quantity integer,
    date date,
    security character varying(250),
    price double precision,
    amount double precision,
    type character varying(250)
);


ALTER TABLE transaction OWNER TO hard_right_edge_app;

--
-- TOC entry 181 (class 1259 OID 74316)
-- Name: transaction_id_seq; Type: SEQUENCE; Schema: public; Owner: hard_right_edge_app
--

CREATE SEQUENCE transaction_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE transaction_id_seq OWNER TO hard_right_edge_app;

--
-- TOC entry 2136 (class 0 OID 0)
-- Dependencies: 181
-- Name: transactions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: hard_right_edge_app
--

ALTER SEQUENCE transaction_id_seq OWNED BY transaction.id;


--
-- TOC entry 1999 (class 2604 OID 74359)
-- Name: id; Type: DEFAULT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY security ALTER COLUMN id SET DEFAULT nextval('security_id_seq'::regclass);


--
-- TOC entry 2000 (class 2604 OID 74376)
-- Name: id; Type: DEFAULT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY security_price ALTER COLUMN id SET DEFAULT nextval('security_price_id_seq'::regclass);


--
-- TOC entry 1998 (class 2604 OID 74321)
-- Name: id; Type: DEFAULT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY transaction ALTER COLUMN id SET DEFAULT nextval('transaction_id_seq'::regclass);


--
-- TOC entry 2004 (class 2606 OID 74361)
-- Name: security_pk; Type: CONSTRAINT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY security
    ADD CONSTRAINT security_pk PRIMARY KEY (id);


--
-- TOC entry 2006 (class 2606 OID 74349)
-- Name: security_platform_pk; Type: CONSTRAINT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY security_platform
    ADD CONSTRAINT security_platform_pk PRIMARY KEY (symbol, platform_id);


--
-- TOC entry 2009 (class 2606 OID 74378)
-- Name: security_price_pk; Type: CONSTRAINT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY security_price
    ADD CONSTRAINT security_price_pk PRIMARY KEY (id);


--
-- TOC entry 2002 (class 2606 OID 74323)
-- Name: transaction_pk; Type: CONSTRAINT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY transaction
    ADD CONSTRAINT transaction_pk PRIMARY KEY (id);


--
-- TOC entry 2007 (class 1259 OID 74384)
-- Name: fki_security_price_security_fk; Type: INDEX; Schema: public; Owner: hard_right_edge_app
--

CREATE INDEX fki_security_price_security_fk ON security_price USING btree (security_id);


--
-- TOC entry 2010 (class 2606 OID 74362)
-- Name: security_platform_security_fk; Type: FK CONSTRAINT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY security_platform
    ADD CONSTRAINT security_platform_security_fk FOREIGN KEY (security_id) REFERENCES security(id);


--
-- TOC entry 2011 (class 2606 OID 74379)
-- Name: security_price_security_fk; Type: FK CONSTRAINT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY security_price
    ADD CONSTRAINT security_price_security_fk FOREIGN KEY (security_id) REFERENCES security(id);


--
-- TOC entry 2132 (class 0 OID 0)
-- Dependencies: 6
-- Name: public; Type: ACL; Schema: -; Owner: postgres
--

REVOKE ALL ON SCHEMA public FROM PUBLIC;
REVOKE ALL ON SCHEMA public FROM postgres;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO PUBLIC;


-- Completed on 2018-04-28 20:22:51

--
-- PostgreSQL database dump complete
--

