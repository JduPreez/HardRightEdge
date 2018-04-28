--
-- PostgreSQL database dump
--

-- Dumped from database version 9.5.3
-- Dumped by pg_dump version 9.5.3

-- Started on 2018-04-28 20:22:48

SET statement_timeout = 0;
SET lock_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SET check_function_bodies = false;
SET client_min_messages = warning;
SET row_security = off;

CREATE ROLE hard_right_edge_app LOGIN ENCRYPTED PASSWORD 'md5cd3df2fc5f80cc9f8881b442fe3582ee'
  CREATEDB
   VALID UNTIL 'infinity';

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


ALTER SCHEMA public OWNER TO postgres;

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
-- Name: share; Type: TABLE; Schema: public; Owner: hard_right_edge_app
--

CREATE TABLE share (
    id bigint NOT NULL,
    name character varying(150) NOT NULL,
    previous_name character varying(150)
);


ALTER TABLE share OWNER TO hard_right_edge_app;

--
-- TOC entry 183 (class 1259 OID 74330)
-- Name: share_id_seq; Type: SEQUENCE; Schema: public; Owner: hard_right_edge_app
--

CREATE SEQUENCE share_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE share_id_seq OWNER TO hard_right_edge_app;

--
-- TOC entry 2134 (class 0 OID 0)
-- Dependencies: 183
-- Name: share_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: hard_right_edge_app
--

ALTER SEQUENCE share_id_seq OWNED BY share.id;


--
-- TOC entry 185 (class 1259 OID 74338)
-- Name: share_platform; Type: TABLE; Schema: public; Owner: hard_right_edge_app
--

CREATE TABLE share_platform (
    share_id bigint NOT NULL,
    platform_id integer NOT NULL,
    symbol character varying(150) NOT NULL
);


ALTER TABLE share_platform OWNER TO hard_right_edge_app;

--
-- TOC entry 187 (class 1259 OID 74373)
-- Name: share_price; Type: TABLE; Schema: public; Owner: hard_right_edge_app
--

CREATE TABLE share_price (
    id integer NOT NULL,
    share_id bigint NOT NULL,
    date date NOT NULL,
    openp double precision NOT NULL,
    high double precision NOT NULL,
    low double precision NOT NULL,
    close double precision NOT NULL,
    adj_close double precision,
    volume double precision NOT NULL
);


ALTER TABLE share_price OWNER TO hard_right_edge_app;

--
-- TOC entry 186 (class 1259 OID 74371)
-- Name: share_price_id_seq; Type: SEQUENCE; Schema: public; Owner: hard_right_edge_app
--

CREATE SEQUENCE share_price_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE share_price_id_seq OWNER TO hard_right_edge_app;

--
-- TOC entry 2135 (class 0 OID 0)
-- Dependencies: 186
-- Name: share_price_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: hard_right_edge_app
--

ALTER SEQUENCE share_price_id_seq OWNED BY share_price.id;


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
-- Name: transactions_id_seq; Type: SEQUENCE; Schema: public; Owner: hard_right_edge_app
--

CREATE SEQUENCE transactions_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE transactions_id_seq OWNER TO hard_right_edge_app;

--
-- TOC entry 2136 (class 0 OID 0)
-- Dependencies: 181
-- Name: transactions_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: hard_right_edge_app
--

ALTER SEQUENCE transactions_id_seq OWNED BY transaction.id;


--
-- TOC entry 1999 (class 2604 OID 74359)
-- Name: id; Type: DEFAULT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY share ALTER COLUMN id SET DEFAULT nextval('share_id_seq'::regclass);


--
-- TOC entry 2000 (class 2604 OID 74376)
-- Name: id; Type: DEFAULT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY share_price ALTER COLUMN id SET DEFAULT nextval('share_price_id_seq'::regclass);


--
-- TOC entry 1998 (class 2604 OID 74321)
-- Name: id; Type: DEFAULT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY transaction ALTER COLUMN id SET DEFAULT nextval('transactions_id_seq'::regclass);


--
-- TOC entry 2004 (class 2606 OID 74361)
-- Name: share_pk; Type: CONSTRAINT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY share
    ADD CONSTRAINT share_pk PRIMARY KEY (id);


--
-- TOC entry 2006 (class 2606 OID 74349)
-- Name: share_platform_pk; Type: CONSTRAINT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY share_platform
    ADD CONSTRAINT share_platform_pk PRIMARY KEY (share_id, platform_id);


--
-- TOC entry 2009 (class 2606 OID 74378)
-- Name: share_price_pk; Type: CONSTRAINT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY share_price
    ADD CONSTRAINT share_price_pk PRIMARY KEY (id);


--
-- TOC entry 2002 (class 2606 OID 74323)
-- Name: transaction_pk; Type: CONSTRAINT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY transaction
    ADD CONSTRAINT transaction_pk PRIMARY KEY (id);


--
-- TOC entry 2007 (class 1259 OID 74384)
-- Name: fki_share_price_share_fk; Type: INDEX; Schema: public; Owner: hard_right_edge_app
--

CREATE INDEX fki_share_price_share_fk ON share_price USING btree (share_id);


--
-- TOC entry 2010 (class 2606 OID 74362)
-- Name: share_platform_share_fk; Type: FK CONSTRAINT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY share_platform
    ADD CONSTRAINT share_platform_share_fk FOREIGN KEY (share_id) REFERENCES share(id);


--
-- TOC entry 2011 (class 2606 OID 74379)
-- Name: share_price_share_fk; Type: FK CONSTRAINT; Schema: public; Owner: hard_right_edge_app
--

ALTER TABLE ONLY share_price
    ADD CONSTRAINT share_price_share_fk FOREIGN KEY (share_id) REFERENCES share(id);


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

