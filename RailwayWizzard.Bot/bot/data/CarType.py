from enum import Enum


class CarType(Enum):
    SEDENTARY = ('Сидячий', True)
    SEDENTARY_BUSINESS = ('Сидячий бизнес', False)
    RESERVED_SEAT_UPPER = ('Плац верх', True)
    RESERVED_SEAT_LOWER = ('Плац низ', True)
    RESERVED_SEAT_UPPER_SIDE = ('Плац верх бок ', False)
    RESERVED_SEAT_LOWER_SIDE = ('Плац низ бок', False)
    COMPARTMENT_UPPER = ('Купе верх', True)
    COMPARTMENT_LOWER = ('Купе низ', True)
    LUXURY = ('СВ', False)

    @property
    def display_name(self):
        return self.value[0]

    @property
    def is_sitting(self):
        return self.value[1]
