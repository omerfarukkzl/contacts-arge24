export interface CsvRowError {
  rowNumber: number;
  field: string;
  message: string;
  rawValue?: string;
}

export interface CsvImportResult {
  importedCount: number;
  failedCount: number;
  errors: CsvRowError[];
}
