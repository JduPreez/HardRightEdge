BEGIN;
	DELETE FROM public.share_price;
	DELETE FROM public.share_platform;
	DELETE FROM public.share;
ROLLBACK; -- COMMIT;