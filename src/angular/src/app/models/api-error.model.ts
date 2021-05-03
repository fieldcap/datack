export class ApiError {
  exception: string;
  stackTrace: string;
  innerExceptions: ApiError[];
}
