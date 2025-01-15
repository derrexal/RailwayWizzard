from dataclasses import dataclass
from typing import List


@dataclass
class NotificationTask:
    DepartureStation: str
    ArrivalStation: str
    DateFrom: str
    TimeFrom: str
    UserId: int
    CarTypes: List[str]
    NumberSeats: int
