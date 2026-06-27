-- ProxyPay Database Creation Script
-- PostgreSQL
-- Generated from EF Core Code First model
--
-- Migration: Run this section to update an existing database
-- ALTER TABLE proxypay_stores ADD COLUMN client_id VARCHAR(32);
-- UPDATE proxypay_stores SET client_id = md5(random()::text) WHERE client_id IS NULL;
-- ALTER TABLE proxypay_stores ALTER COLUMN client_id SET NOT NULL;
-- CREATE UNIQUE INDEX ix_proxypay_stores_client_id ON proxypay_stores (client_id);
-- ALTER TABLE proxypay_stores ADD COLUMN billing_strategy INTEGER NOT NULL DEFAULT 1;
-- UPDATE proxypay_billings b SET billing_strategy = 1 WHERE billing_strategy IS NOT NULL;
-- ALTER TABLE proxypay_billings DROP COLUMN IF EXISTS billing_strategy;
-- CREATE TABLE IF NOT EXISTS proxypay_billing_items (
--     billing_item_id BIGSERIAL NOT NULL,
--     billing_id BIGINT NOT NULL,
--     description VARCHAR(500) NOT NULL,
--     quantity INTEGER NOT NULL,
--     unit_price DOUBLE PRECISION NOT NULL,
--     discount DOUBLE PRECISION NOT NULL DEFAULT 0,
--     created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
--     CONSTRAINT proxypay_billing_items_pkey PRIMARY KEY (billing_item_id),
--     CONSTRAINT fk_proxypay_billing_item_billing FOREIGN KEY (billing_id) REFERENCES proxypay_billings (billing_id) ON DELETE CASCADE
-- );

CREATE TABLE proxypay_stores (
    store_id BIGSERIAL NOT NULL,
    client_id VARCHAR(32) NOT NULL,
    user_id BIGINT NOT NULL,
    name VARCHAR(240) NOT NULL,
    email VARCHAR(240),
    billing_strategy INTEGER NOT NULL DEFAULT 1,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT proxypay_stores_pkey PRIMARY KEY (store_id)
);

CREATE UNIQUE INDEX ix_proxypay_stores_client_id ON proxypay_stores (client_id);
CREATE INDEX ix_proxypay_stores_user_id ON proxypay_stores (user_id);

CREATE TABLE proxypay_customers (
    customer_id BIGSERIAL NOT NULL,
    store_id BIGINT,
    name VARCHAR(240) NOT NULL,
    document_id VARCHAR(20),
    cellphone VARCHAR(20),
    email VARCHAR(240),
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT proxypay_customers_pkey PRIMARY KEY (customer_id),
    CONSTRAINT fk_proxypay_customer_store FOREIGN KEY (store_id) REFERENCES proxypay_stores (store_id)
);

CREATE TABLE proxypay_invoices (
    invoice_id BIGSERIAL NOT NULL,
    customer_id BIGINT,
    store_id BIGINT,
    invoice_number VARCHAR(50) NOT NULL,
    notes TEXT,
    status INTEGER NOT NULL DEFAULT 1,
    payment_method INTEGER NOT NULL DEFAULT 1,
    discount DOUBLE PRECISION NOT NULL DEFAULT 0,
    due_date TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    external_code VARCHAR(240),
    expires_at TIMESTAMP WITHOUT TIME ZONE,
    paid_at TIMESTAMP WITHOUT TIME ZONE,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT proxypay_invoices_pkey PRIMARY KEY (invoice_id),
    CONSTRAINT fk_proxypay_invoice_customer FOREIGN KEY (customer_id) REFERENCES proxypay_customers (customer_id),
    CONSTRAINT fk_proxypay_invoice_store FOREIGN KEY (store_id) REFERENCES proxypay_stores (store_id)
);

CREATE UNIQUE INDEX ix_proxypay_invoices_number ON proxypay_invoices (invoice_number);

CREATE TABLE proxypay_invoice_items (
    invoice_item_id BIGSERIAL NOT NULL,
    invoice_id BIGINT NOT NULL,
    description VARCHAR(500) NOT NULL,
    quantity INTEGER NOT NULL,
    unit_price DOUBLE PRECISION NOT NULL,
    discount DOUBLE PRECISION NOT NULL DEFAULT 0,
    total DOUBLE PRECISION NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT proxypay_invoice_items_pkey PRIMARY KEY (invoice_item_id),
    CONSTRAINT fk_proxypay_invoice_item_invoice FOREIGN KEY (invoice_id) REFERENCES proxypay_invoices (invoice_id) ON DELETE CASCADE
);

CREATE TABLE proxypay_billings (
    billing_id BIGSERIAL NOT NULL,
    store_id BIGINT,
    customer_id BIGINT,
    frequency INTEGER NOT NULL,
    payment_method INTEGER NOT NULL DEFAULT 1,
    billing_start_date TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    status INTEGER NOT NULL DEFAULT 1,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT proxypay_billings_pkey PRIMARY KEY (billing_id),
    CONSTRAINT fk_proxypay_billing_store FOREIGN KEY (store_id) REFERENCES proxypay_stores (store_id),
    CONSTRAINT fk_proxypay_billing_customer FOREIGN KEY (customer_id) REFERENCES proxypay_customers (customer_id)
);

CREATE TABLE proxypay_billing_items (
    billing_item_id BIGSERIAL NOT NULL,
    billing_id BIGINT NOT NULL,
    description VARCHAR(500) NOT NULL,
    quantity INTEGER NOT NULL,
    unit_price DOUBLE PRECISION NOT NULL,
    discount DOUBLE PRECISION NOT NULL DEFAULT 0,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT proxypay_billing_items_pkey PRIMARY KEY (billing_item_id),
    CONSTRAINT fk_proxypay_billing_item_billing FOREIGN KEY (billing_id) REFERENCES proxypay_billings (billing_id) ON DELETE CASCADE
);

CREATE TABLE proxypay_transactions (
    transaction_id BIGSERIAL NOT NULL,
    invoice_id BIGINT,
    store_id BIGINT,
    type INTEGER NOT NULL,
    category INTEGER NOT NULL,
    description VARCHAR(500) NOT NULL,
    amount DOUBLE PRECISION NOT NULL,
    balance DOUBLE PRECISION NOT NULL,
    created_at TIMESTAMP WITHOUT TIME ZONE NOT NULL,
    CONSTRAINT proxypay_transactions_pkey PRIMARY KEY (transaction_id),
    CONSTRAINT fk_proxypay_transaction_invoice FOREIGN KEY (invoice_id) REFERENCES proxypay_invoices (invoice_id),
    CONSTRAINT fk_proxypay_transaction_store FOREIGN KEY (store_id) REFERENCES proxypay_stores (store_id)
);
