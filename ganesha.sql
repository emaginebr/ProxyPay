-- Ganesha Database Creation Script
-- PostgreSQL
-- Generated from EF Core Code First model

CREATE TABLE ganesha_stores (
    store_id BIGSERIAL NOT NULL,
    slug VARCHAR(120) NOT NULL,
    name VARCHAR(120) NOT NULL,
    owner_id BIGINT NOT NULL,
    logo VARCHAR(150),
    status INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT ganesha_stores_pkey PRIMARY KEY (store_id)
);

CREATE UNIQUE INDEX ix_ganesha_stores_slug ON ganesha_stores (slug);

CREATE TABLE ganesha_invoices (
    invoice_id BIGSERIAL NOT NULL,
    user_id BIGINT NOT NULL,
    invoice_number VARCHAR(50) NOT NULL,
    notes TEXT,
    status INTEGER NOT NULL DEFAULT 1,
    sub_total DOUBLE PRECISION NOT NULL,
    discount DOUBLE PRECISION NOT NULL DEFAULT 0,
    tax DOUBLE PRECISION NOT NULL DEFAULT 0,
    total DOUBLE PRECISION NOT NULL,
    due_date TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    paid_at TIMESTAMP WITHOUT TIME ZONE,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT ganesha_invoices_pkey PRIMARY KEY (invoice_id)
);

CREATE UNIQUE INDEX ix_ganesha_invoices_number ON ganesha_invoices (invoice_number);

CREATE TABLE ganesha_invoice_items (
    invoice_item_id BIGSERIAL NOT NULL,
    invoice_id BIGINT NOT NULL,
    description VARCHAR(500) NOT NULL,
    quantity INTEGER NOT NULL,
    unit_price DOUBLE PRECISION NOT NULL,
    discount DOUBLE PRECISION NOT NULL DEFAULT 0,
    total DOUBLE PRECISION NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT ganesha_invoice_items_pkey PRIMARY KEY (invoice_item_id),
    CONSTRAINT fk_ganesha_invoice_item_invoice FOREIGN KEY (invoice_id) REFERENCES ganesha_invoices (invoice_id) ON DELETE CASCADE
);

CREATE TABLE ganesha_transactions (
    transaction_id BIGSERIAL NOT NULL,
    user_id BIGINT NOT NULL,
    invoice_id BIGINT,
    type INTEGER NOT NULL,
    category INTEGER NOT NULL,
    description VARCHAR(500) NOT NULL,
    amount DOUBLE PRECISION NOT NULL,
    balance DOUBLE PRECISION NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT ganesha_transactions_pkey PRIMARY KEY (transaction_id),
    CONSTRAINT fk_ganesha_transaction_invoice FOREIGN KEY (invoice_id) REFERENCES ganesha_invoices (invoice_id)
);

CREATE INDEX ix_ganesha_transactions_user_id ON ganesha_transactions (user_id);
