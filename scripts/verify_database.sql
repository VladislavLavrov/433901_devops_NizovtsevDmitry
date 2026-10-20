SELECT ID_DataInputVariant, RequestId, Operand_1, Operand_2, Type_operation, Result
FROM `13-WebCalculatorDb`.`DataInputVariants`
ORDER BY ID_DataInputVariant DESC LIMIT 30;

SELECT * FROM `13-WebCalculatorDb`.`__EFMigrationsHistory`;

-- New messages must not create duplicate DB records for the same non-null GUID.
SELECT RequestId, COUNT(*) AS copies
FROM `13-WebCalculatorDb`.`DataInputVariants`
WHERE RequestId IS NOT NULL
GROUP BY RequestId HAVING COUNT(*) > 1;
