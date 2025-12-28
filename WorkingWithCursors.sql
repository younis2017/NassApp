USE TEMPDB
GO

CREATE TABLE Employee
(
 EmpID int IDENTITY(1,1) PRIMARY KEY,
 EmpName varchar (50) NOT NULL,
 Salary int NOT NULL,
 Address varchar (200) NOT NULL,
)
GO
SET NOCOUNT ON;

INSERT INTO Employee(EmpName,Salary,Address) VALUES('Ford McMaster',55000,'135 Queens Plate Blvs.')
INSERT INTO Employee(EmpName,Salary,Address) VALUES('Alex Davison',51500,'120 King St. West')
INSERT INTO Employee(EmpName,Salary,Address) VALUES('Cindy Allen',62000,'9190 Lawrence Ave West')
INSERT INTO Employee(EmpName,Salary,Address) VALUES('Sam Solomon',52700,'500 Bloor St. West')
INSERT INTO Employee(EmpName,Salary,Address) VALUES('James Lee',65500,'990 Kipling Ave.')
GO

SELECT * FROM Employee 
GO
--/*
SET NOCOUNT ON
DECLARE @Id int
DECLARE @name varchar(50)
DECLARE @salary int

DECLARE cur_emp CURSOR
STATIC FOR SELECT EmpID,EmpName,Salary from Employee ORDER BY EmpID

OPEN cur_emp
IF @@CURSOR_ROWS > 0
BEGIN 
	FETCH NEXT FROM cur_emp INTO @Id,@name,@salary
	WHILE @@Fetch_status = 0
	BEGIN
		if @id = 2
			INSERT INTO Employee(EmpName,Salary,Address) VALUES('Adam Adams',911911,'991 Kipling Ave.')

		IF @name='Ford McMaster'
			Update Employee SET Salary=95000 where EmpID = @Id

		PRINT convert(varchar(40),getdate(),114) + ' ID : '+ convert(varchar(20),@Id)+', Name : '+@name+ ', Salary : '+convert(varchar(20),@salary)
		WAITFOR DELAY '00:00:01';
		FETCH NEXT FROM cur_emp INTO @Id,@name,@salary
	END
END

CLOSE cur_emp
DEALLOCATE cur_emp
GO
SET NOCOUNT ON;
SELECT * FROM Employee 
GO
--*/

-----------------------------------------------
--/*
PRINT '-------------- TEST 2 --------------------------'
 --Dynamic Cursor for Update
SET NOCOUNT ON
DECLARE @Id int
DECLARE @name varchar(50)
DECLARE @salary int

DECLARE Dynamic_cur_empupdate CURSOR DYNAMIC 
FOR SELECT EmpID,EmpName,Salary from Employee ORDER BY EmpID -- Try by EmpName OR DESC

OPEN Dynamic_cur_empupdate

IF @@CURSOR_ROWS <> 0
BEGIN 
	FETCH NEXT FROM Dynamic_cur_empupdate INTO @Id, @name, @salary
	WHILE @@Fetch_status = 0
	BEGIN

	if @id = 4
		INSERT INTO Employee(EmpName,Salary,Address) VALUES('Rob Robinson',119119,'119 Kipling Ave.')

	PRINT convert(varchar(40),getdate(),114) + ' ID : '+ convert(varchar(20),@Id)+', Name : '+@name+ ', Salary : '+convert(varchar(20),@salary)

	--Update Employee SET Salary=95000 WHERE CURRENT OF Dynamic_cur_empupdate

	--IF @name='Alex Davison'
		--DELETE Employee WHERE CURRENT OF Dynamic_cur_empupdate
	
	WAITFOR DELAY '00:00:02';

	FETCH NEXT FROM Dynamic_cur_empupdate INTO @Id,@name, @salary
 END
END

CLOSE Dynamic_cur_empupdate
DEALLOCATE Dynamic_cur_empupdate
SET NOCOUNT OFF
Go
SET NOCOUNT ON;

Select * from Employee 
--*/

DROP TABLE Employee
GO