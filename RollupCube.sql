USE tempdb;
GO

CREATE TABLE employee
  (
      id INT PRIMARY KEY,
      name VARCHAR(50) NOT NULL,
      gender VARCHAR(50) NOT NULL,
      salary INT NOT NULL,
      department VARCHAR(50) NOT NULL
   )
GO

INSERT INTO employee
  VALUES
  (1, 'David', 'Male', 5000, 'Sales'),
  (2, 'Jim', 'Female', 6000, 'HR'),
  (3, 'Kate', 'Female', 7500, 'IT'),
  (4, 'Will', 'Male', 6500, 'Marketing'),
  (5, 'Shane', 'Female', 5500, 'Finance'),
  (6, 'Shed', 'Male', 8000, 'Sales'),
  (7, 'Vik', 'Male', 7200, 'HR'),
  (8, 'Vince', 'Female', 6600, 'IT'),
  (9, 'Jane', 'Female', 5400, 'Marketing'),
  (10, 'Laura', 'Female', 6300, 'Finance'),
  (11, 'Mac', 'Male', 5700, 'Sales'),
  (12, 'Pat', 'Male', 7000, 'HR'),
  (13, 'Julie', 'Female', 7100, 'IT'),
  (14, 'Elice', 'Female', 6800,'Marketing'),
  (15, 'Wayne', 'Male', 5000, 'Finance')
GO


SELECT department, sum(salary) as Salary_Sum
FROM employee
GROUP BY department
GO

SELECT department, sum(salary) as Salary_Sum
FROM employee
GROUP BY department
UNION
SELECT 'Grand Total', sum(salary) as Salary_Sum
FROM employee
GO


SELECT department,
sum(salary) as Salary_Sum
FROM employee
GROUP BY ROLLUP (department)
GO










---------------------------------------------------------------------------------------

SELECT coalesce (department, 'Total for Departments') AS Department,
sum(salary) as Salary_Sum
FROM employee
GROUP BY ROLLUP (department)
GO
----------------------------------------------------------------------------------------


SELECT
coalesce (department, 'All Departments') AS Department,
coalesce (gender,'All Genders') AS Gender,
sum(salary) as Salary_Sum
FROM employee
GROUP BY ROLLUP (department, gender)
GO
-----------------------------------------------------------------------------------------


SELECT
coalesce (department, 'All Departments') AS Department,
coalesce (gender,'All Genders') AS Gender,
sum(salary) as Salary_Sum
FROM employee
GROUP BY CUBE (department, gender)
GO
------------------------------------------------------------------------------------------

drop table employee
GO