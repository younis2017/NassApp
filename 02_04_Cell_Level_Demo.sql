-- Creates a database master key encrypted by password 'StrongPassword'
CREATE MASTER KEY ENCRYPTION BY PASSWORD  = 'StrongPassword'
GO

-- Creates an asymmetric key encrypted by password 'Password'
CREATE ASYMMETRIC KEY MyAsymmetricKey
WITH ALGORITHM = RSA_2048 ENCRYPTION BY PASSWORD = 'Password'
GO

-- Creates an symmetric key encrypted by asymmetric key
CREATE SYMMETRIC KEY MySymmetricKey
WITH ALGORITHM = AES_256
ENCRYPTION BY ASYMMETRIC KEY MyAsymmetricKey
GO
 
CREATE TABLE TestEncryption (
     [Name] [varchar](256)
    ,[CreditCardNumber] [varchar](16)
    ,[EncryptedCreditCardNumber] [varbinary](max)
    )
GO
 
INSERT INTO TestEncryption (
    [Name]
    ,[CreditCardNumber]
    )
VALUES ('Donald Duck','1234567890123456'),
	   ('Mickey Mouse','9876543210987654')
GO
 
 -- Opening the symmetric key
OPEN SYMMETRIC KEY MySymmetricKey
DECRYPTION BY ASYMMETRIC KEY MyAsymmetricKey
WITH PASSWORD  = 'Password'
GO

UPDATE TestEncryption
SET [EncryptedCreditCardNumber] = ENCRYPTBYKEY(KEY_GUID('MySymmetricKey'), CreditCardNumber)
GO

SELECT *
FROM [TestEncryption]
GO

CLOSE SYMMETRIC KEY MySymmetricKey
select name, CreditCardNumber, convert(varchar(16), decryptbykey(EncryptedCreditCardNumber)) 
as CCNumber from TestEncryption;

---

OPEN SYMMETRIC KEY MySymmetricKey
DECRYPTION BY ASYMMETRIC KEY MyAsymmetricKey
WITH PASSWORD  = 'Password'

-- Performs the update of the record
INSERT INTO TestEncryption (name, CreditCardNumber, EncryptedCreditCardNumber)
VALUES ('Henry Ford','3503503503500', EncryptByKey( Key_GUID('mySymmetricKey'),'3503503503500'));    

select name, convert(varchar(10), decryptbykey(EncryptedCreditCardNumber)) 
as CCNumber from TestEncryption;

CLOSE SYMMETRIC KEY MySymmetricKey

GO

drop table TestEncryption;
drop SYMMETRIC KEY MySymmetricKey;
drop ASYMMETRIC KEY MyAsymmetricKey;
drop MASTER KEY;