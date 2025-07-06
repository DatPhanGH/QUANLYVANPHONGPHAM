
-- thủ tục tạo mã phiếu nhập tự động
CREATE PROCEDURE SP_GetNextMaPN 
    @NxtMaPN VARCHAR(10) OUTPUT
AS
BEGIN
    DECLARE @MaxMaPN VARCHAR(10);
    DECLARE @NextNumber INT;

    -- Lấy giá trị MaPN lớn nhất
    SELECT @MaxMaPN = MAX(MaPN) FROM PHIEUNHAP;

    -- Nếu bảng trống, gán giá trị mặc định
    IF @MaxMaPN IS NULL
        SET @NxtMaPN = 'PN001';
    ELSE
    BEGIN
        -- Trích xuất và chuyển đổi phần số của MaPN
        SET @NextNumber = CAST(SUBSTRING(@MaxMaPN, 3, 3) AS INT) + 1;

        -- Tạo giá trị MaPN tiếp theo
        SET @NxtMaPN = 'PN' + RIGHT('00' + CAST(@NextNumber AS VARCHAR(3)), 3);
    END
END
-- gọi mã PN mới
DECLARE @NewMaPN VARCHAR(10)
EXEC SP_GetNextMaPN @NewMaPN OUTPUT
SELECT @NewMaPN

--trigger tăng số lượng tồn mỗi khi nhập hàng
CREATE TRIGGER Tr_CapNhatSoLuongTon
ON ChiTietPhieuNhap
AFTER INSERT
AS
BEGIN
    UPDATE SanPham
    SET SoLuongTon = SoLuongTon + (select SoLuong from inserted)
    FROM SanPham s
    INNER JOIN inserted i ON s.MaSP = i.MaSP;
END;