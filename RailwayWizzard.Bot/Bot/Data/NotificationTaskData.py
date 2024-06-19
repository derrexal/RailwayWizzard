from dataclasses import dataclass
from typing import List


@dataclass
class NotificationTaskData:
    DepartureStation: str
    ArrivalStation: str
    DateFrom: str
    TimeFrom: str
    UserId: int
    CarTypes: List[str]
    NumberSeats: int
